import json
from freq import *

letterFreq = {"a":8.497, "b":1.492, "c":2.202, "d":4.253, "e":11.162, "f":2.228,
              "g":2.015, "h":6.094, "i":7.546, "j":0.153, "k":1.292, "l":4.025,
              "m":2.406, "n":6.749, "o":7.507, "p":1.929, "q":0.095, "r":7.587,
              "s":6.327, "t":9.356, "u":2.758, "v":0.978, "w":2.56, "x":0.15,
              "y":1.994, "z":0.077}

reducedInput = ["a",";","s","l","d","k","f","j"]
# ken1 [ 'os', 'zjkxqap', 'efw', 'mnh', 'vicg', 'ld', 'yur', 'bt' ],
mappingKen1 = [ 'os', 'zjkxqap', 'efw', 'mnh', 'vicg', 'ld', 'yur', 'bt' ]
mappingKen2 = [ 'fok', 'vbe', 'lzxquw', 'ims', 'tr', 'gya', 'pjd', 'hcn' ]
mappingKen3 = [ 'dwfq', 'hnp', 'uzs', 'jyir', 'ckga', 'vbe', 'lt', 'xom' ]
mappingKen4 = [ 'figm', 'vs', 'odc', 'zlyu', 'jewr', 'bqt', 'xpa', 'knh' ]
mappingKens = [mappingKen1, mappingKen2, mappingKen3, mappingKen4]

def calculateLetterFreq(mappingKen1):
    fingerFreqs = {}
    for section in mappingKen1:
        fingerFreq = 0
        for letter in section:
            fingerFreq += letterFreq[letter]
        fingerFreqs[section] = fingerFreq
    # sortedFingerFreqs = OrderedDict(sorted(fingerFreqs.items()))
    sortedFingerFreqs = {k: v for k, v in sorted(fingerFreqs.items(), key=lambda item: item[1])}
#     assign it to each fingers: ri, li, rm, lm, rr, lr, rp, lp
    newConfig = {}
    for idx, item in enumerate(sortedFingerFreqs):
        for letter in item:
            newConfig[letter] = reducedInput[idx]
    return newConfig

if __name__ == '__main__':
    noswear10k = feed_vocabulary()

    for idx,eachmapping in enumerate(mappingKens):
        configKen = calculateLetterFreq(eachmapping)
        print(json.dumps(configKen, indent=4))
        f = open("config" + "kenmapping" + str(idx) + ".json", "w")
        json.dump(configKen, f)

        process_config(noswear10k, configKen, "config" + "kenmapping" + str(idx))