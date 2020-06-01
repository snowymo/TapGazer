# generate frequency with the word given and write to file as json
import json
import re
from wordfreq import *

def generateWordFreq(filename):
    phrasesDict = []
    with open(filename, encoding="utf-8") as f:
        phrasesDict = f.read().splitlines()

    wordFreqPair = {}
    for item in phrasesDict:
        item = re.sub('\s+', '', item)
        wordFreqPair[item] = word_frequency(item, "en", "large")

    with open(filename.split(".")[0] + "-freq.json", 'w') as outfile:
        json.dump(wordFreqPair, outfile)

if __name__ == '__main__':
    generateWordFreq("top40k.txt")