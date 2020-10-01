# This is a sample Python script.

# Press Shift+F10 to execute it or replace it with your code.
# Press Double Shift to search everywhere for classes, files, tool windows, actions, and settings.

import numpy as np
from nltk.tokenize import RegexpTokenizer
from keras.models import Sequential, load_model
from keras.layers import LSTM
from keras.layers.core import Dense, Activation
from keras.optimizers import RMSprop
import matplotlib.pyplot as plt
import pickle
import heapq
import os.path
from os import path
import time

def print_hi(name):
    # Use a breakpoint in the code line below to debug your script.
    print(f'Hi, {name}')  # Press Ctrl+F8 to toggle the breakpoint.

WORD_LENGTH = 5

text = open('1661-0.txt', encoding='utf-8').read().lower()
print('corpus length:', len(text))

global tokenizer
tokenizer = RegexpTokenizer(r'\w+')
words = tokenizer.tokenize(text)

global unique_words
unique_words = np.unique(words)
global unique_word_index
unique_word_index = dict((c, i) for i, c in enumerate(unique_words))

prev_words = []
next_words = []
for i in range(len(words) - WORD_LENGTH):
    prev_words.append(words[i:i + WORD_LENGTH])
    next_words.append(words[i + WORD_LENGTH])
# print(prev_words[0])
# print(next_words[0])

global X
X = np.zeros((len(prev_words), WORD_LENGTH, len(unique_words)), dtype=bool)
global Y
Y = np.zeros((len(next_words), len(unique_words)), dtype=bool)
for i, each_words in enumerate(prev_words):
    for j, each_word in enumerate(each_words):
        X[i, j, unique_word_index[each_word]] = 1
    Y[i, unique_word_index[next_words[i]]] = 1

if not path.exists("keras_next_word_model.h5"):
    model = Sequential()
    model.add(LSTM(128, input_shape=(WORD_LENGTH, len(unique_words))))
    model.add(Dense(len(unique_words)))
    model.add(Activation('softmax'))

    optimizer = RMSprop(lr=0.01)
    model.compile(loss='categorical_crossentropy', optimizer=optimizer, metrics=['accuracy'])
    history = model.fit(X, Y, validation_split=0.05, batch_size=128, epochs=2, shuffle=True).history

    model.save('keras_next_word_model.h5')
    pickle.dump(history, open("history.p", "wb"))

model = load_model('keras_next_word_model.h5')

def prepare_input(text):
    x = np.zeros((1, WORD_LENGTH, len(unique_words)))
    for t, word in enumerate(text.split()):
        # print(word)
        x[0, t, unique_word_index[word]] = 1
    return x

def sample(preds, top_n=3):
    preds = np.asarray(preds).astype('float64')
    preds = np.log(preds)
    exp_preds = np.exp(preds)
    preds = exp_preds / np.sum(exp_preds)

    return heapq.nlargest(top_n, range(len(preds)), preds.take)

def predict_completions(text, n=3):
    if text == "":
        return("0")
    x = prepare_input(text)


    # history = pickle.load(open("history.p", "rb"))

    preds = model.predict(x, verbose=0)[0]
    next_indices = sample(preds, n)
    return [unique_words[idx] for idx in next_indices]

def eval():
    q = "Your life will never be the same again"
    q = "It is not a lack of love but a lack of friendship that makes unhappy marriages"
    seq = " ".join(tokenizer.tokenize(q.lower())[5:10])

    print("correct sentence: ", q)
    print("Sequence: ", seq)
    print("next possible words: ", predict_completions(seq, 5))

# Press the green button in the gutter to run the script.
if __name__ == '__main__':
    print_hi('PyCharm')
    eval()

