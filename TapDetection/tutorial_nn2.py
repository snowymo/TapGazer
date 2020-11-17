import torch
from torch.utils.data import DataLoader
from torch import nn
from torch import optim
from datetime import datetime
from torch.utils.data import TensorDataset
import torch.nn.functional as F
import numpy as np
import pickle

# DATA
def get_data(train_ds, valid_ds, bs):
    return (
        DataLoader(train_ds, batch_size=bs, shuffle=True),
        DataLoader(valid_ds, batch_size=bs),
    )

class WrappedDataLoader:
    def __init__(self, dl, func):
        self.dl = dl
        self.func = func

    def __len__(self):
        return len(self.dl)

    def __iter__(self):
        batches = iter(self.dl)
        for b in batches:
            yield (self.func(*b))

# CPU OR GPU
dev = torch.device(
    "cuda") if torch.cuda.is_available() else torch.device("cpu")
# Let’s update preprocess to move batches to the GPU:
# not really necessary here
def preprocess(x, y):
    # print(x.shape)
    x = torch.unsqueeze(x, dim=1)
    # print(x.shape)
    # return x.view(-1, -1, 1).to(dev), y.to(dev)
    return x.to(dev), y.to(dev)

# to load all kinds of func
class Lambda(nn.Module):
    def __init__(self, func):
        super().__init__()
        self.func = func

    def forward(self, x):
        return self.func(x)

# input data pair
# x_train is the input, aka acc of one finger,
# y_train is the target, aka bool of contact
# another idea is, input all of them and output the finger ID
x_train = pickle.load( open( "x_train.pkl", "rb" ) ).astype(np.float32)
y_train = pickle.load( open( "y_train.pkl", "rb" ) ).astype(np.int_).flatten()
x_valid = pickle.load( open( "x_valid.pkl", "rb" ) ).astype(np.float32)
y_valid = pickle.load( open( "y_valid.pkl", "rb" ) ).astype(np.int_).flatten()
print(x_train.shape, y_train.shape)
x_train, x_valid = map(
    torch.tensor, (x_train, x_valid)
)
y_train, y_valid = map(
    torch.LongTensor, (y_train, y_valid)
)

train_ds = TensorDataset(x_train, y_train)
# data for validation
valid_ds = TensorDataset(x_valid, y_valid)
# batch size
bs = 16 # I can try different ones

train_dl, valid_dl = get_data(train_ds, valid_ds, bs)
train_dl = WrappedDataLoader(train_dl, preprocess)
valid_dl = WrappedDataLoader(valid_dl, preprocess)
# we can move our model to the GPU.
# MODEL DESIGN
model = nn.Sequential(
    nn.Conv1d(in_channels=1,out_channels=8,kernel_size=3),# output bs x 8 x 13=104
    # nn.Conv1d(in_channels=64,out_channels=128,kernel_size=3),
    nn.BatchNorm1d(8),
    nn.Flatten(),
    nn.Linear(104, 104),
    nn.ReLU(),
    nn.Linear(104, 64),
    # nn.Conv2d(16, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Linear(64,32),
    # nn.Conv1d(5,1,kernel_size=2),
    # nn.Conv2d(16, 10, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Linear(32,16),
    nn.Linear(16,6),
    nn.Linear(6,6),
    # nn.AdaptiveAvgPool2d(1),
    # Lambda(lambda x: x.view(x.size(0), -1)),
)
model2 = nn.Sequential(
    nn.Linear(15,10),
    nn.ReLU(),
    nn.Linear(10,6),
    # nn.Conv2d(16, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Linear(6,6),
    # nn.Conv1d(5,1,kernel_size=2),
    # nn.Conv2d(16, 10, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Linear(6,6),
    # nn.AdaptiveAvgPool2d(1),
    # Lambda(lambda x: x.view(x.size(0), -1)),
)
model.to(dev)

# If you’re using negative log likelihood loss and log softmax activation,
# then Pytorch provides a single function F.cross_entropy that combines the two.
# So we can even remove the activation function from our model.

loss_func = F.cross_entropy
lr = 0.03  # learning rate
epochs = 10  # how many epochs to train for
opt = optim.SGD(model.parameters(), lr=lr, momentum=0.9)
# We’ll now do a little refactoring of our own.
# Since we go through a similar process twice of calculating the loss for both the training set and the validation set,
# let’s make that into its own function, loss_batch, which computes the loss for one batch.

# We pass an optimizer in for the training set, and use it to perform backprop.
# For the validation set, we don’t pass an optimizer, so the method doesn’t perform backprop.
def loss_batch(model, loss_func, xb, yb, opt=None):
    modelxb = model(xb)
    loss = loss_func(modelxb, yb)

    if opt is not None:
        loss.backward()
        opt.step()
        opt.zero_grad()

    return loss.item(), len(xb)

def accuracy(out, yb):
    preds = torch.argmax(out, dim=1)
    return (preds == yb).float().mean()

def fit(epochs, model, loss_func, opt, train_dl, valid_dl):
    for epoch in range(epochs):
        # train mode
        model.zero_grad()
        model.train()
        iteration = 0
        for xb, yb in train_dl:
            iteration += 1
            train_loss,_ = loss_batch(model, loss_func, xb, yb, opt)
            if iteration % 1 == 0:
                print("train_loss", iteration, train_loss)

        acc = accuracy(model(xb), yb)
        print("acc", epoch, acc)
        # evaluation mode
        model.eval()
        with torch.no_grad():
            losses, nums = zip(
                *[loss_batch(model, loss_func, xb, yb) for xb, yb in valid_dl]
            )
        val_loss = np.sum(np.multiply(losses, nums)) / np.sum(nums)

        print("val_loss", epoch, val_loss)

curTime = datetime.now()
fit(epochs, model, loss_func, opt, train_dl, valid_dl)
print("Current Time =", str(datetime.now()-curTime))