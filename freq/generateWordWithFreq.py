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

    newfilename = filename.split(".txt")[0] + "-freq.json"
    with open(newfilename, 'w') as outfile:
        json.dump(wordFreqPair, outfile)
    print("write into " + newfilename)

def generateWordFreqForKen(filename):
    phrasesDict = []
    with open(filename, encoding="utf-8") as f:
        phrasesDict = f.read().splitlines()

    newfilename = filename.split(".txt")[0] + "-freq-ken.js"
    with open(newfilename, 'w') as outfile:
        outfile.write("let words = {\n")
        for item in phrasesDict:
            item = re.sub('\s+', '', item)
            outfile.write("\"" + item + "\":" + str("%.2f" % zipf_frequency(item, "en", "large")) + ",\n")
        outfile.write("};")
    print("write into " + newfilename)

if __name__ == '__main__':
    # generateWordFreq("top40k.txt")
    # generateWordFreqForKen("top40k.txt")
    generateWordFreq("top0.9.txt")
    # print (zipf_frequency("participant", "en", "large"))