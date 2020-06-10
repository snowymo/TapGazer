# figure out the first letter and last letter
from wordfreq import *

firstletter = {}
lastletter = {}
for i in range(0,26):
    firstletter[str(""+chr(97+i))]=0
    lastletter[str("" + chr(97+i))] = 0

with open('top40k.txt', 'r', encoding='utf-8') as filehandle:
    Lines = filehandle.readlines()
    for line in Lines:
        line = line.lower()
        line = line.replace('\n', '')
        firstletter[""+line[0]] += word_frequency(line, "en", "large")
        lastletter[""+line[len(line) - 1]] += word_frequency(line, "en", "large")

# split into three bins
firstKnife1 = 7
firstKnife2 = 16
lastKnife1 = 5
lastKnife2 = 17
# 
leftFirstBin = 0
middleFirstBin = 0
rightFirstBin = 0
leftLastBin = 0
middleLastBin = 0
rightLastBin = 0
for i in range(0, firstKnife1):
    leftFirstBin += firstletter[""+chr(97+i)]
for i in range(firstKnife1, firstKnife2):
    middleFirstBin += firstletter[""+chr(97+i)]
for i in range(firstKnife2, 26):
    rightFirstBin += firstletter[""+chr(97+i)]

for i in range(0, lastKnife1):
    leftLastBin += lastletter[""+chr(97+i)]
for i in range(lastKnife1, lastKnife2):
    middleLastBin += lastletter[""+chr(97+i)]
for i in range(lastKnife2, 26):
    rightLastBin += lastletter[""+chr(97+i)]

print(leftFirstBin, middleFirstBin, rightFirstBin)
print(leftLastBin, middleLastBin, rightLastBin)