# MNIST data setup
from pathlib import Path
import requests

DATA_PATH = Path("data")
PATH = DATA_PATH / "mnist"

PATH.mkdir(parents=True, exist_ok=True)

URL = "http://deeplearning.net/data/mnist/"
FILENAME = "mnist.pkl.gz"

if not (PATH / FILENAME).exists():
        content = requests.get(URL + FILENAME).content
        (PATH / FILENAME).open("wb").write(content)

# This dataset is in numpy array format, and has been stored using pickle, a python-specific format for serializing data.
import pickle
import gzip

with gzip.open((PATH / FILENAME).as_posix(), "rb") as f:
        ((x_train, y_train), (x_valid, y_valid), _) = pickle.load(f, encoding="latin-1")

# Each image is 28 x 28, and is being stored as a flattened row of length 784 (=28x28). Let’s take a look at one; we need to reshape it to 2d first.
from matplotlib import pyplot
import numpy as np

pyplot.imshow(x_train[0].reshape((28, 28)), cmap="gray")
print(x_train.shape, y_train.shape)
pyplot.show()

# PyTorch uses torch.tensor, rather than numpy arrays, so we need to convert our data.
import torch

x_train, y_train, x_valid, y_valid = map(
    torch.tensor, (x_train, y_train, x_valid, y_valid)
)
n, c = x_train.shape
print("n", n, "c", c)
# x_train, x_train.shape, y_train.min(), y_train.max()
# print("x_train", x_train, "y_train", y_train)
print("x_train.shape", x_train.shape)
print("y_train.min()", y_train.min(), "y_train.max()", y_train.max())#?

########## Neural net from scratch (no torch.nn)
import math

weights = torch.randn(784, 10) / math.sqrt(784)
weights.requires_grad_()#?
bias = torch.zeros(10, requires_grad=True) #Thanks to PyTorch’s ability to calculate gradients automatically, we can use any standard Python function (or callable object) as a model!

# We also need an activation function, so we’ll write log_softmax and use it.
# Remember: although PyTorch provides lots of pre-written loss functions, activation functions, and so forth,
# you can easily write your own using plain python. PyTorch will even create fast GPU or vectorized CPU code for your function automatically.
def log_softmax(x):
    return x - x.exp().sum(-1).log().unsqueeze(-1)

def model(xb):
    return log_softmax(xb @ weights + bias) # the @ stands for the dot product operation

bs = 64  # batch size

xb = x_train[0:bs]  # a mini-batch from x
preds = model(xb)  # predictions
# As you see, the preds tensor contains not only the tensor values, but also a gradient function. We’ll use this later to do backprop.
print("xb.shape", xb.shape, "preds[0]", preds[0], "preds.shape", preds.shape)

# Let’s implement negative log-likelihood to use as the loss function (again, we can just use standard Python)
def nll(input, target):
    return -input[range(target.shape[0]), target].mean()

loss_func = nll
# Let’s check our loss with our random model, so we can see if we improve after a backprop pass later.
yb = y_train[0:bs]
print("yb.shape", yb.shape, yb, loss_func(preds, yb))

# Let’s also implement a function to calculate the accuracy of our model.
# For each prediction, if the index with the largest value matches the target value, then the prediction was correct.
def accuracy(out, yb):
    preds = torch.argmax(out, dim=1)
    return (preds == yb).float().mean()

print("accuracy", accuracy(preds, yb))

# We can now run a training loop.
lr = 0.5  # learning rate
epochs = 3  # how many epochs to train for

for epoch in range(epochs):
    for i in range((n - 1) // bs + 1):
        #         set_trace()
        start_i = i * bs
        end_i = start_i + bs
        xb = x_train[start_i:end_i]
        yb = y_train[start_i:end_i]
        pred = model(xb)
        loss = loss_func(pred, yb)

        loss.backward()
        with torch.no_grad():
            weights -= weights.grad * lr
            bias -= bias.grad * lr
            weights.grad.zero_()
            bias.grad.zero_()

    print("loss_func", loss_func(model(xb), yb), "accuracy", accuracy(model(xb), yb))

#######Using torch.nn.functional

# The first and easiest step is to make our code shorter by replacing our hand-written activation and loss functions with those from torch.nn.functional
# (which is generally imported into the namespace F by convention).
# This module contains all the functions in the torch.nn library (whereas other parts of the library contain classes).
# As well as a wide range of loss and activation functions, you’ll also find here some convenient functions for creating neural nets, such as pooling functions.
# (There are also functions for doing convolutions, linear layers, etc, but as we’ll see, these are usually better handled using other parts of the library.)
#
# If you’re using negative log likelihood loss and log softmax activation,
# then Pytorch provides a single function F.cross_entropy that combines the two.
# So we can even remove the activation function from our model.
import torch.nn.functional as F
print("using NN")
loss_func = F.cross_entropy
def model(xb):
    return xb @ weights + bias
print(loss_func(model(xb), yb), accuracy(model(xb), yb))

# nn.Module (uppercase M) is a PyTorch specific concept,
# and is a class we’ll be using a lot. nn.Module is not to be confused with the Python concept of a (lowercase m) module,
# which is a file of Python code that can be imported.

from torch import nn

class Mnist_Logistic(nn.Module):
    def __init__(self):
        super().__init__()
        self.weights = nn.Parameter(torch.randn(784, 10) / math.sqrt(784))
        self.bias = nn.Parameter(torch.zeros(10))

    def forward(self, xb):
        return xb @ self.weights + self.bias

#     Since we’re now using an object instead of just using a function, we first have to instantiate our model:
model = Mnist_Logistic()
# Now we can calculate the loss in the same way as before.
# Note that nn.Module objects are used as if they are functions (i.e they are callable),
# but behind the scenes Pytorch will call our forward method automatically.
print("===Using torch.nn.functional===", loss_func(model(xb), yb))

# We’ll wrap our little training loop in a fit function so we can run it again later.
def fit():
    for epoch in range(epochs):
        for i in range((n - 1) // bs + 1):
            start_i = i * bs
            end_i = start_i + bs
            xb = x_train[start_i:end_i]
            yb = y_train[start_i:end_i]
            pred = model(xb)
            loss = loss_func(pred, yb)

            loss.backward()
            #  we can take advantage of model.parameters() and model.zero_grad()
            #  (which are both defined by PyTorch for nn.Module) to make those steps more concise
            #  and less prone to the error of forgetting some of our parameters, particularly if we had a more complicated model:
            with torch.no_grad():
                    for p in model.parameters():
                            p -= p.grad * lr
                    model.zero_grad()
    print(loss_func(model(xb), yb), "accuracy", accuracy(model(xb), yb))

fit()

# Refactor using nn.Linear
# We continue to refactor our code.
# Instead of manually defining and initializing self.weights and self.bias, and calculating xb  @ self.weights + self.bias,
# we will instead use the Pytorch class nn.Linear for a linear layer, which does all that for us.
# Pytorch has many types of predefined layers that can greatly simplify our code, and often makes it faster too.
class Mnist_Logistic(nn.Module):
    def __init__(self):
        super().__init__()
        self.lin = nn.Linear(784, 10)

    def forward(self, xb):
        return self.lin(xb)

model = Mnist_Logistic()
print("===nn.Linear===", loss_func(model(xb), yb))
fit()

########### Refactor using optim
# Pytorch also has a package with various optimization algorithms, torch.optim.
# We can use the step method from our optimizer to take a forward step,
# instead of manually updating each parameter.

from torch import optim
# We’ll define a little function to create our model and optimizer so we can reuse it in the future.
def get_model():
    model = Mnist_Logistic()
    return model, optim.SGD(model.parameters(), lr=lr)
model, opt = get_model()
print("===Refactor using optim===", loss_func(model(xb), yb))

for epoch in range(epochs):
    for i in range((n - 1) // bs + 1):
        start_i = i * bs
        end_i = start_i + bs
        xb = x_train[start_i:end_i]
        yb = y_train[start_i:end_i]
        pred = model(xb)
        loss = loss_func(pred, yb)

        loss.backward()
        opt.step()
        opt.zero_grad()
    print(loss_func(model(xb), yb), "accuracy", accuracy(model(xb), yb))

############Refactor using Dataset
# PyTorch has an abstract Dataset class.
# A Dataset can be anything that has a __len__ function (called by Python’s standard len function)
# and a __getitem__ function as a way of indexing into it.
from torch.utils.data import TensorDataset
# Both x_train and y_train can be combined in a single TensorDataset, which will be easier to iterate over and slice.
train_ds = TensorDataset(x_train, y_train)
# Now, we can do these two steps together:
# xb,yb = train_ds[i*bs : i*bs+bs]
model, opt = get_model()
print("===Refactor using Dataset===", loss_func(model(xb), yb))
for epoch in range(epochs):
    for i in range((n - 1) // bs + 1):
        xb, yb = train_ds[i * bs: i * bs + bs]
        pred = model(xb)
        loss = loss_func(pred, yb)

        loss.backward()
        opt.step()
        opt.zero_grad()
    print(loss_func(model(xb), yb), "accuracy", accuracy(model(xb), yb))


########## Refactor using DataLoader
# Rather than having to use train_ds[i*bs : i*bs+bs], the DataLoader gives us each minibatch automatically.
from torch.utils.data import DataLoader

train_ds = TensorDataset(x_train, y_train)
train_dl = DataLoader(train_ds, batch_size=bs)
# Now, our loop is much cleaner, as (xb, yb) are loaded automatically from the data loader:
# for xb,yb in train_dl:
#     pred = model(xb)
model, opt = get_model()
print("===Refactor using DataLoader===", loss_func(model(xb), yb))
for epoch in range(epochs):
    for xb, yb in train_dl:
        pred = model(xb)
        loss = loss_func(pred, yb)

        loss.backward()
        opt.step()
        opt.zero_grad()
    print(loss_func(model(xb), yb), "accuracy", accuracy(model(xb), yb))

#### Validation
# In section 1, we were just trying to get a reasonable training loop set up for use on our training data.
# In reality, you always should also have a validation set, in order to identify if you are overfitting.

# it makes no sense to shuffle the validation data.

# We’ll use a batch size for the validation set that is twice as large as that for the training set.
# This is because the validation set does not need backpropagation and thus takes less memory (it doesn’t need to store the gradients).
# We take advantage of this to use a larger batch size and compute the loss more quickly.
train_ds = TensorDataset(x_train, y_train)
train_dl = DataLoader(train_ds, batch_size=bs, shuffle=True)

valid_ds = TensorDataset(x_valid, y_valid)
valid_dl = DataLoader(valid_ds, batch_size=bs * 2)
# We will calculate and print the validation loss at the end of each epoch.
model, opt = get_model()
print("=====Validation=====")
epochs = 6  # how many epochs to train for
for epoch in range(epochs):
    #     sets the module in training mode
    model.train()
    for xb, yb in train_dl:
        pred = model(xb)
        loss = loss_func(pred, yb)

        loss.backward()
        opt.step()
        opt.zero_grad()
    #     sets the module in evaluation mode
    model.eval()
    with torch.no_grad():
        valid_loss = sum(loss_func(model(xb), yb) for xb, yb in valid_dl)

    print(epoch, valid_loss / len(valid_dl))

# We’ll now do a little refactoring of our own.
# Since we go through a similar process twice of calculating the loss for both the training set and the validation set,
# let’s make that into its own function, loss_batch, which computes the loss for one batch.

# We pass an optimizer in for the training set, and use it to perform backprop.
# For the validation set, we don’t pass an optimizer, so the method doesn’t perform backprop.
def loss_batch(model, loss_func, xb, yb, opt=None):
    # modelxb = model(xb)
    # print(modelxb.shape)
    loss = loss_func(model(xb), yb)

    if opt is not None:
        loss.backward()
        opt.step()
        opt.zero_grad()

    return loss.item(), len(xb)

import numpy as np
def fit(epochs, model, loss_func, opt, train_dl, valid_dl):
    for epoch in range(epochs):
        model.train()
        for xb, yb in train_dl:
            loss_batch(model, loss_func, xb, yb, opt)

        model.eval()
        with torch.no_grad():
            losses, nums = zip(
                *[loss_batch(model, loss_func, xb, yb) for xb, yb in valid_dl]
            )
        val_loss = np.sum(np.multiply(losses, nums)) / np.sum(nums)

        print(epoch, val_loss)

def get_data(train_ds, valid_ds, bs):
    return (
        DataLoader(train_ds, batch_size=bs, shuffle=True),
        DataLoader(valid_ds, batch_size=bs * 2),
    )

#
print("=====Create fit() and get_data()=====")
train_dl, valid_dl = get_data(train_ds, valid_ds, bs)
model, opt = get_model()
fit(epochs=1, model=model, loss_func=loss_func, opt=opt, train_dl=train_dl, valid_dl=valid_dl)


# ##########Let’s see if we can use them to train a convolutional neural network (CNN)!
# We will use Pytorch’s predefined Conv2d class as our convolutional layer.
# We define a CNN with 3 convolutional layers. Each convolution is followed by a ReLU.
# At the end, we perform an average pooling.
class Mnist_CNN(nn.Module):
    def __init__(self):
        super().__init__()
        self.conv1 = nn.Conv2d(1, 16, kernel_size=3, stride=2, padding=1)
        self.conv2 = nn.Conv2d(16, 16, kernel_size=3, stride=2, padding=1)
        self.conv3 = nn.Conv2d(16, 10, kernel_size=3, stride=2, padding=1)

    def forward(self, xb):
        xb = xb.view(-1, 1, 28, 28)
        xb = F.relu(self.conv1(xb))
        xb = F.relu(self.conv2(xb))
        xb = F.relu(self.conv3(xb))
        xb = F.avg_pool2d(xb, 4)#?
        return xb.view(-1, xb.size(1))

lr = 0.1
# Momentum is a variation on stochastic gradient descent
# that takes previous updates into account as well and generally leads to faster training.
model = Mnist_CNN()
opt = optim.SGD(model.parameters(), lr=lr, momentum=0.9)#SGD?
print("===CNN===")
epochs = 1
fit(epochs, model, loss_func, opt, train_dl, valid_dl)

from datetime import datetime
# nn.Sequential
# A Sequential object runs each of the modules contained within it, in a sequential manner.
# This is a simpler way of writing our neural network.
# To take advantage of this, we need to be able to easily define a custom layer from a given function.
# For instance, PyTorch doesn’t have a view layer, and we need to create one for our network.
# Lambda will create a layer that we can then use when defining a network with Sequential.
class Lambda(nn.Module):
    def __init__(self, func):
        super().__init__()
        self.func = func

    def forward(self, x):
        return self.func(x)

def preprocess(x):
    return x.view(-1, 1, 28, 28)

model = nn.Sequential(
    Lambda(preprocess),
    nn.Conv2d(1, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Conv2d(16, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Conv2d(16, 10, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.AvgPool2d(4),
    Lambda(lambda x: x.view(x.size(0), -1)),
)
print("====NN sequential====")
opt = optim.SGD(model.parameters(), lr=lr, momentum=0.9)
epochs = 1
fit(epochs, model, loss_func, opt, train_dl, valid_dl)

### wrapping data loader===
# Let’s get rid of these two assumptions, so our model works with any 2d single channel image.
# First, we can remove the initial Lambda layer but moving the data preprocessing into a generator:
def preprocess(x, y):
    return x.view(-1, 1, 28, 28), y
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

train_dl, valid_dl = get_data(train_ds, valid_ds, bs)
train_dl = WrappedDataLoader(train_dl, preprocess)
valid_dl = WrappedDataLoader(valid_dl, preprocess)
# Next, we can replace nn.AvgPool2d with nn.AdaptiveAvgPool2d,
# which allows us to define the size of the output tensor we want,
# rather than the input tensor we have. As a result, our model will work with any size input.
model = nn.Sequential(
    nn.Conv2d(1, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Conv2d(16, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Conv2d(16, 10, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.AdaptiveAvgPool2d(1),
    Lambda(lambda x: x.view(x.size(0), -1)),
)
print("====wrapping dataloader====")
opt = optim.SGD(model.parameters(), lr=lr, momentum=0.9)
curTime = datetime.now()
fit(epochs, model, loss_func, opt, train_dl, valid_dl)
print("Current Time =", str(curTime-datetime.now()))

############ GPU
# have access to a CUDA-capable GPU
# print(torch.cuda.is_available())
# create a device object for it
dev = torch.device(
    "cuda") if torch.cuda.is_available() else torch.device("cpu")
# Let’s update preprocess to move batches to the GPU:
def preprocess(x, y):
    return x.view(-1, 1, 28, 28).to(dev), y.to(dev)

train_dl, valid_dl = get_data(train_ds, valid_ds, bs)
train_dl = WrappedDataLoader(train_dl, preprocess)
valid_dl = WrappedDataLoader(valid_dl, preprocess)
# we can move our model to the GPU.
model = nn.Sequential(
    nn.Conv2d(1, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Conv2d(16, 16, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.Conv2d(16, 10, kernel_size=3, stride=2, padding=1),
    nn.ReLU(),
    nn.AdaptiveAvgPool2d(1),
    Lambda(lambda x: x.view(x.size(0), -1)),
)
model.to(dev)
print("======GPU=====")
opt = optim.SGD(model.parameters(), lr=lr, momentum=0.9)
curTime = datetime.now()
epochs = 6
fit(epochs, model, loss_func, opt, train_dl, valid_dl)
print("Current Time =", str(datetime.now()-curTime))