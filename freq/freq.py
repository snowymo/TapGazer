#https://pypi.org/project/wordfreq/
#
import functools
import json
from wordfreq import *
from collections import OrderedDict
from termcolor import colored


config = {'q': 'a', 'a': 'a', 'z': 's',
    'w': 's', 's': 's', 'x': 's',
    'e': 'd', 'd': 'd', 'c': 'd',
    'r': 'f', 'f': 'f', 'v': 'f',
    't': 'f', 'g': 'f', 'b': 'f',
    'y': 'j', 'h': 'j', 'n': 'j',
    'u': 'j', 'j': 'j', 'm': 'j',
    'i': 'k', 'k': 'k',
    'o': 'l', 'l': 'l',
    'p': ';'}

configP = {"a": "s", "s": "s", "x": "s", "z": "s", "e": "d", "f": "d", "q": "d", "r": "d", "w": "d", "c": "f", "d": "f", "v": "f", "b": "j", "g": "j", "h": "j", "j": "j", "m": "j", "n": "j", "t": "j", "y": "j", "i": "k", "k": "k", "o": "k", "p": "k", "u": "k", "l": "l"}
# try nine fingers
configAZ = {"a":"a","b":"a","c":"a",
          "d":"s","e":"s","f":"s",
          "g":"d","h":"d","i":"d",
          "j":"f","k":"f","l":"f",
          "m":"g","n":"g","o":"g",
          "p":"j","q":"j","r":"j",
          "s":"k","t":"k","u":"k",
          "v":"l","w":"l","x":"l",
          "y":";","z":";"}
# '19 Quadmetric Optimized Thumb-to-Finger Interaction for Force Assisted One-Handed Text Entry on Mobile Headsets
configQ = {"t":"a","u":"a","f":"a",
          "a":"s","y":"s","w":"s",
          "s":"d","j":"d","d":"d",
          "c":"f","v":"f","r":"f",
          "i":"g","q":"g","e":"g",
          "p":"j","k":"j","l":"j",
          "o":"k","x":"k","h":"k",
          "b":"l","z":"l","n":"l",
          "m":";","g":";"}
# qwerty -> 3x9
config3x9 = {"q":"a","w":"a","e":"a",
          "r":"s","t":"s","y":"s",
          "u":"d","i":"d","o":"d",
          "a":"f","s":"f","d":"f",
          "f":"g","g":"g","h":"g",
          "j":"j","k":"j","p":"j",
          "z":"k","x":"k","c":"k",
          "v":"l","b":"l","n":"l",
          "m":";","l":";"}
# T9
configT9 = {"a":"a","b":"a","c":"a",
          "d":"s","e":"s","f":"s",
          "g":"d","h":"d","i":"d",
          "j":"f","k":"f","l":"f",
          "m":"j","n":"j","o":"j",
          "p":"k","q":"k","r":"k","s":"k",
          "t":"l","u":"l","v":"l",
          "w":";","x":";","y":";","z":";"}
# Dvorak
configDvorak = {"a":"a",
                "o":"s","q":"s",
                "e":"d","j":"d",
                "p":"f","u":"f","k":"f","y":"f","i":"f","x":"f",
                "f":"j","d":"j","b":"j","g":"j","h":"j","m":"j",
                "c":"k","t":"k","w":"k",
                "r":"l","n":"l","v":"l",
                "l":";","s":";","z":";"}
# colemak
configColemak = {"q":"a","a":"a","z":"a",
                 "w":"s","r":"s","x":"s",
                 "f":"d","s":"d","c":"d",
                 "p":"f","t":"f","v":"f","g":"f","d":"f","b":"f",
                 "j":"j","h":"j","k":"j","l":"j","n":"j","m":"j",
                 "u":"k","e":"k",
                 "y":"l","i":"l",
                 "o":";"}


configKen1 = {"m":"a","n":"a","h":"a",
              "o":"s","s":"s",
              "v":"d","i":"d","c":"d","g":"d",
              "e":"f","f":"f","w":"f",
              "z":"j","j":"j","k":"j","x":"j","q":"j","a":"j","p":"j",
              "y":"k","u":"k","r":"k",
              "b":"l","t":"l",
              "l":";","d":";"}

freq_dict = get_frequency_dict("en", wordlist='best')

dictionaryFileName = "top0.9"

tapping_dict = {}
word_rank = {}
completed_numbers = {}
configFileName = "classic"
def change_config():
    # use regular input to change the configuration
    print("Do you want to change the configuration? Default key mapping is: ")
    print("qaz;\twsx;\tedc;\trfvtgb;\tyhnuj;\ttikm;\tol;\tp")
    print("Type y if you want to, type anything else if you prefer the default one")
    answer = input()
    mapFinger2Name = {'a': "left pinky",
                      's': "left ring",
                      'd': "left middle",
                      'f': "left index",
                      'j': "right index",
                      'k': "right middle",
                      'l': "right ring",
                      ';': "right pinky",}
    global config
    global configFileName
    if answer == "y":
        # change the config
        print("name your config file")
        configFileName = input()
        config = {}
        for entry in mapFinger2Name:
            print("type your " + mapFinger2Name[entry] + " keys, press enter when you finished")
            keys = input()
            for currentKey in keys:
                config[currentKey] = entry
    print("new config ")
    print(json.dumps(config, indent = 4))
    f = open("config" + configFileName + ".json", "w")
    json.dump(config, f)


def find_rank(word, dict):
    for idx, item in enumerate(dict):
        if item == word:
            return idx

    return -1

def find_finger(key, config=config):
    key = key.lower()
    if key in config:
        return config[key]
    else:
        return None

def add_to_map(typing, word):
    # add word to the value of keyword typing
    if typing in tapping_dict:
        tapping_dict[typing][word] = word_frequency(word, 'en')#freq_dict[word.lower()]
    else:
        tapping_dict[typing] = {word: word_frequency(word, 'en')}#freq_dict[word.lower()]}

def generate_tap_map(test_dict, count, config=config):
    # go through the dictionary (300k words), add each one to its one-tap two-tap until n-tap buckets, and sort it with the freq
    cur_count = 0
    largest_cand_number = 0
    largest_cand_input_string = ""
    for word in test_dict:
        word = word.lower()
        if len(word) == 0:
            continue

        # if the word already showed up as upper case or lower case before, ignore it
        if word in word_rank:
            # print(word + " exists already")
            continue

        not_supported = False
        # print("processing", word)
        cur_typing = ""
        for _, char in enumerate(word):
            # find the corresponding finger based on the char
            cur_finger = find_finger(char, config)
            if cur_finger is None:
                not_supported = True
                break

            cur_typing += cur_finger
            add_to_map(cur_typing, word)

        if not_supported is False:
            # sorted(tapping_dict[cur_typing], reverse=True)
            word_rank[word] = len(tapping_dict[cur_typing])
            if len(word) == len(cur_typing):
                if cur_typing in completed_numbers:
                    completed_numbers[cur_typing].append(word)
                    # record the largest list number in this dictionary
                    if len(completed_numbers[cur_typing]) > largest_cand_number:
                        largest_cand_number = len(completed_numbers[cur_typing])
                        largest_cand_input_string = cur_typing
                else:
                    completed_numbers[cur_typing] = [word]
            # if len(word) == len(cur_typing) and word_rank[word] > 9 and len(word) > 1:
            #     print("dangerous word\t" + cur_typing +"\t"+ word +"\t"+ str(word_rank[word]))
            if cur_count == count:
                break
            cur_count = cur_count + 1

    #     check if we have more than 10-homograph
    # with open("phrases2.txt", encoding="utf-8") as f:
    #     phrasesDict = f.read().splitlines()
    for cur_typing in completed_numbers:
        if len(completed_numbers[cur_typing]) > 10:
            print("[risk]" + cur_typing + " has more than 10 homographs " + str(len(completed_numbers[cur_typing])))
            print(*completed_numbers[cur_typing], sep = ", ")
    #         check phrase2.txt
    #         for index in range(10, len(completed_numbers[cur_typing])):
    #             if completed_numbers[cur_typing][index] in phrasesDict:
    #                 print("{bcolors.WARNING}[very risk]" + completed_numbers[cur_typing][index] + "{bcolors.ENDC}")


    # print("largest_cand_result", largest_cand_number, largest_cand_input_string, completed_numbers[largest_cand_input_string])
    # for item in completed_numbers:
    #     if len(completed_numbers[item]) > 5:
    #         print(item + "\t" + str(len(completed_numbers[item])) + "\t" + str(completed_numbers[item]))
    print("\n")
    # sort each item in tapping_dict
    for tapping in tapping_dict:
        sorted(tapping, reverse = True)

def compare(item1, item2):
    return word_frequency(item2.lower(), 'en') - word_frequency(item1.lower(), 'en')

def reset_containers():
    global word_rank
    global tapping_dict
    global completed_numbers
    tapping_dict = {}
    word_rank = {}
    completed_numbers = {}

def check_with_phrases(filename, config=config):
    # try to output the candidate ranking for all the words in phrase2.txt
    with open(filename, encoding="utf-8") as f:
        phrasesDict = f.read().splitlines()
    minimumCandNum = {}
    minimumCandNum["2"] = 0
    minimumCandNum["3"] = 0
    minimumCandNum["4"] = 0
    for idx1, item in enumerate(phrasesDict):
        currentPhrase = item.lower().split()
        # go through currentPhrase
        for idx2, currentWord in enumerate(currentPhrase):
            # check the rank of currentWord
            # add missing word in phrases2.txt to dict?
            cur_typing = ""
            for _, char in enumerate(currentWord):
                # find the corresponding finger based on the char
                cur_finger = find_finger(char, config)
                if cur_finger is None:
                    print("\t\tword not supported: " + currentWord)
                    break
                cur_typing += cur_finger
            if len(cur_typing) != len(currentWord):
                continue
            inputString = cur_typing
            # go through the result of inputString in completed_numbers,
            # if currentWord == "nor":
            #     print(currentWord)
            if inputString not in completed_numbers:
                print(currentWord + " not in the dictionary")
                # print(currentWord)
                continue
            for idx3, candidate in enumerate(completed_numbers[inputString]):
                if candidate.lower() == currentWord.lower():
                    # idx3 is the rank
                    if idx3 > 10:
                        print(colored("word: " + currentWord + " : " + str(idx3), 'red'))
                    if len(inputString) == 2:
                        minimumCandNum["2"] = max(minimumCandNum["2"], idx3)
                    elif len(inputString) == 3:
                        minimumCandNum["3"] = max(minimumCandNum["3"], idx3)
                    else:
                        minimumCandNum["4"] = max(minimumCandNum["4"], idx3)
                    break
    print(minimumCandNum)

def process_config(noswear10k, curConfig, configFileName):
    reset_containers()
    global config
    config = curConfig
    count = len(noswear10k)
    generate_tap_map(noswear10k, count, config)
    #
    print("\n\n====config:" + configFileName + "====")

    print("\ncheck " + dictionaryFileName + ".txt")
    check_with_phrases(dictionaryFileName + '.txt', config)

    print("\ncheck 1k.txt")
    check_with_phrases('1k.txt', config)

    print("\ncheck phrases2.txt")
    check_with_phrases('phrases2.txt', config)
    # write to file
    f = open(dictionaryFileName + "-result" + configFileName + ".txt", "w")
    f.write(str(tapping_dict))
    f.close()
    f = open(dictionaryFileName + "-cand" + configFileName + ".txt", "w")
    f.write(str(completed_numbers))
    f.close()

    # try json so javascript maybe can read it directly

    # save the result without freq
    inputstring_word_map = {}
    for inputstring in tapping_dict:
        inputstring_word_map[inputstring] = [*tapping_dict[inputstring].keys()]
    with open(dictionaryFileName + '-result' + configFileName + '.json', 'w') as fp:
        json.dump(inputstring_word_map, fp)

    with open(dictionaryFileName + '-cand' + configFileName + '.json', 'w') as fp:
        json.dump(completed_numbers, fp)

    # write profile.name
    pn = open("profile.name", 'w')
    pn.write(configFileName)
    pn.close()


def feed_vocabulary():
    print("\n" + dictionaryFileName + ".txt")
    with open(dictionaryFileName + '.txt', encoding="utf-8") as f:
        noswear10k = f.read().splitlines()
    # remove spaces
    noswear10k = [line.replace(' ', '') for line in noswear10k]
    noswear10k = [line.replace('\t', '') for line in noswear10k]
    for idx, item in enumerate(noswear10k):
        noswear10k[idx] = noswear10k[idx].lower()
    count = len(noswear10k)
    print("original " + dictionaryFileName + ".txt has " + str(count) + " words")
    # add phrase2.txt
    with open('phrases2.txt', encoding="utf-8") as f:
        phrasesDict = f.read().splitlines()
    for item in phrasesDict:
        currentPhrase = item.lower().split()
        noswear10k.extend(currentPhrase)
    count = len(noswear10k)
    print("new dict has " + str(count) + " words")

    sorted(noswear10k, key=functools.cmp_to_key(compare))
    return noswear10k

if __name__ == "__main__":
    # word: a Unicode string containing the word to look up. Ideally the word is a single token according to our tokenizer, but if not, there is still hope -- see Tokenization below.
    # lang: the BCP 47 or ISO 639 code of the language to use, such as 'en'.
    # wordlist: which set of word frequencies to use. Current options are 'small', 'large', and 'best'.
    # minimum: If the word is not in the list or has a frequency lower than minimum, return minimum instead. You may want to set this to the minimum value contained in the wordlist, to avoid a discontinuity where the wordlist ends.
    # word_frequency(word, lang, wordlist='best', minimum=0.0)
    freq_cafe = word_frequency('that', 'en')
    print("that freq", freq_cafe)
    freq_cafe = word_frequency('for', 'en')
    print("for freq", freq_cafe)

    # zipf_frequency is a variation on word_frequency that aims to return the word frequency on a human-friendly logarithmic scale.
    freq_the = zipf_frequency('the', 'en')

    top_10_en = top_n_list('en', 10)

    iter = iter_wordlist("en", wordlist='best') #iterates through all the words in a wordlist, in descending frequency order.

    count = 1000
    # not using dict, too many weird words. Let's use wiki-100k.txt
    # we need to feed the word in the order of freq, to save time for sorting
    # print("processing wiki100k.txt")
    # with open('wiki-100k.txt', encoding="utf-8") as f:
    #     wiki100k = f.read().splitlines()
    # # remove spaces
    # wiki100k = [line.replace(' ', '') for line in wiki100k]
    # wiki100k = [line.replace('\t', '') for line in wiki100k]
    # wiki100k = [line.replace('\'', '') for line in wiki100k]
    # for idx, item in enumerate(wiki100k):
    #     wiki100k[idx] = wiki100k[idx].lower()
    # count = len(wiki100k)
    # # resort
    # sorted(wiki100k, key=functools.cmp_to_key(compare))
    # generate_tap_map(wiki100k, count)
    # # write to file
    # f = open("wiki100k-result.txt", "w")
    # f.write(str(tapping_dict))
    # f.close()
    # f = open("wiki100k-cand.txt", "w")
    # f.write(str(completed_numbers))
    # f.close()

    # customized configuration
    change_config()

    # google-10000-english-usa-no-swears
    # reset_containers()
    # print("\nprocessing 10000-no-swear.txt")
    # with open('google-10000-english-usa-no-swears.txt', encoding="utf-8") as f:
    #     noswear10k = f.read().splitlines()
    # # remove spaces
    # noswear10k = [line.replace(' ', '') for line in noswear10k]
    # noswear10k = [line.replace('\t', '') for line in noswear10k]
    # for idx, item in enumerate(noswear10k):
    #     noswear10k[idx] = noswear10k[idx].lower()
    # count = len(noswear10k)
    # sorted(noswear10k, key=functools.cmp_to_key(compare))
    # generate_tap_map(noswear10k, count)
    # #
    # check_with_phrases()
    # # write to file
    # f = open("noswear10k-result" + configFileName + ".txt", "w")
    # f.write(str(tapping_dict))
    # f.close()
    # f = open("noswear10k-cand.txt", "w")
    # f.write(str(completed_numbers))
    # f.close()
    # # try json so javascript maybe can read it directly
    #
    # # save the result without freq
    # inputstring_word_map = {}
    # for inputstring in tapping_dict:
    #     inputstring_word_map[inputstring] = [*tapping_dict[inputstring].keys()]
    # with open('noswear10k-result' + configFileName + '.json', 'w') as fp:
    #     json.dump(inputstring_word_map, fp)

    # 30k.txt
    reset_containers()
    print("\n" + dictionaryFileName + ".txt")
    with open(dictionaryFileName + '.txt', encoding="utf-8") as f:
        noswear10k = f.read().splitlines()
    # remove spaces
    # noswear10k = [line.replace(' ', '') for line in noswear10k]
    # noswear10k = [line.replace('\t', '') for line in noswear10k]
    # for idx, item in enumerate(noswear10k):
    #     noswear10k[idx] = noswear10k[idx].lower()
    # count = len(noswear10k)
    # print("original 30k.txt has " + str(count) + " words")
    # # add phrase2.txt
    # with open('phrases2.txt', encoding="utf-8") as f:
    #     phrasesDict = f.read().splitlines()
    # for item in phrasesDict:
    #     currentPhrase = item.lower().split()
    #     noswear10k.extend(currentPhrase)
    # count = len(noswear10k)
    # print("new dict has " + str(count) + " words")
    #
    # sorted(noswear10k, key=functools.cmp_to_key(compare))

    noswear10k = feed_vocabulary()

    process_config(noswear10k, config, configFileName)







