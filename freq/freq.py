#https://pypi.org/project/wordfreq/
#
import functools
import json
from wordfreq import *

config = {'q': 'a', 'a': 'a', 'z': 'a',
    'w': 's', 's': 's', 'x': 's',
    'e': 'd', 'd': 'd', 'c': 'd',
    'r': 'f', 'f': 'f', 'v': 'f',
    't': 'f', 'g': 'f', 'b': 'f',
    'y': 'j', 'h': 'j', 'n': 'j',
    'u': 'j', 'j': 'j', 'm': 'j',
    'i': 'k', 'k': 'k',
    'o': 'l', 'l': 'l',
    'p': ';'}

freq_dict = get_frequency_dict("en", wordlist='best')

tapping_dict = {}
word_rank = {}
completed_numbers = {}
configFileName = ""
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

def find_finger(key):
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

def generate_tap_map(test_dict, count):
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
            cur_finger = find_finger(char)
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
    print("largest_cand_result", largest_cand_number, largest_cand_input_string, completed_numbers[largest_cand_input_string])
    for item in completed_numbers:
        if len(completed_numbers[item]) > 5:
            print(item + "\t" + str(len(completed_numbers[item])) + "\t" + str(completed_numbers[item]))
    print("\n")
    # sort each item in tapping_dict
    for tapping in tapping_dict:
        sorted(tapping, reverse = True)

def compare(item1, item2):
    return word_frequency(item2.lower(), 'en') - word_frequency(item1.lower(), 'en')

def reset_containers():
    tapping_dict = {}
    word_rank = {}
    completed_numbers = {}

def check_with_phrases(filename):
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
                cur_finger = find_finger(char)
                cur_typing += cur_finger
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
                    if idx3 > 7:
                        print("word: " + currentWord + " : " + str(idx3))
                        if len(inputString) == 2:
                            minimumCandNum["2"] = max(minimumCandNum["2"], idx3)
                        elif len(inputString) == 3:
                            minimumCandNum["3"] = max(minimumCandNum["3"], idx3)
                        else:
                            minimumCandNum["4"] = max(minimumCandNum["4"], idx3)
                    break
    print(minimumCandNum)

if __name__ == "__main__":
    # word: a Unicode string containing the word to look up. Ideally the word is a single token according to our tokenizer, but if not, there is still hope -- see Tokenization below.
    # lang: the BCP 47 or ISO 639 code of the language to use, such as 'en'.
    # wordlist: which set of word frequencies to use. Current options are 'small', 'large', and 'best'.
    # minimum: If the word is not in the list or has a frequency lower than minimum, return minimum instead. You may want to set this to the minimum value contained in the wordlist, to avoid a discontinuity where the wordlist ends.
    # word_frequency(word, lang, wordlist='best', minimum=0.0)
    freq_cafe = word_frequency('ok', 'en')
    print("ok freq", freq_cafe)

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
    print("\n30k.txt")
    with open('30k.txt', encoding="utf-8") as f:
        noswear10k = f.read().splitlines()
    # remove spaces
    noswear10k = [line.replace(' ', '') for line in noswear10k]
    noswear10k = [line.replace('\t', '') for line in noswear10k]
    for idx, item in enumerate(noswear10k):
        noswear10k[idx] = noswear10k[idx].lower()
    count = len(noswear10k)
    print("original 30k.txt has " + str(count) + " words")
    # add phrase2.txt
    with open('phrases2.txt', encoding="utf-8") as f:
        phrasesDict = f.read().splitlines()
    for item in phrasesDict:
        currentPhrase = item.lower().split()
        noswear10k.extend(currentPhrase)
    count = len(noswear10k)
    print("new dict has " + str(count) + " words")

    sorted(noswear10k, key=functools.cmp_to_key(compare))
    generate_tap_map(noswear10k, count)
    #
    print("check phrases2.txt")
    check_with_phrases('phrases2.txt')
    print("check 30k.txt")
    check_with_phrases('30k.txt')
    # write to file
    f = open("30k-result" + configFileName + ".txt", "w")
    f.write(str(tapping_dict))
    f.close()
    f = open("30k-cand" + configFileName + ".txt", "w")
    f.write(str(completed_numbers))
    f.close()

    # try json so javascript maybe can read it directly

    # save the result without freq
    inputstring_word_map = {}
    for inputstring in tapping_dict:
        inputstring_word_map[inputstring] = [*tapping_dict[inputstring].keys()]
    with open('30k-result' + configFileName + '.json', 'w') as fp:
        json.dump(inputstring_word_map, fp)

    with open('30k-cand' + configFileName + '.json', 'w') as fp:
        json.dump(completed_numbers, fp)






