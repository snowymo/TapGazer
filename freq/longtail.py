# top_n_list(lang, n, wordlist='best')

from wordfreq import *
import re

top10k = top_n_list('en', 1000000, wordlist="large")
print(len(top10k))

top40k =top_n_list('en', 40145, wordlist="large")

missing_words = ["if", "our", "then"]
mw_freq = 0
for mw in missing_words:
    print( mw, word_frequency(mw,"en", "large"))
    mw_freq += word_frequency(mw,"en", "large")
print(mw_freq)


with open('top40k.txt', 'w', encoding='utf-8') as filehandle:
    for word in top40k:
        word = word.lower()
        if re.match('^[a-z]+$',word):
            if len(word) > 3 or word_frequency(word, "en", "large") > word_frequency("confessed", "en"):
                filehandle.writelines("%s\n" % word)

def get_freq_until(prob):
    sum = 0
    count = 0
    with open('top' + str(prob) + '.txt', 'w', encoding='utf-8') as filehandle:
        for item in top10k:
            item = item.lower()
            # no matter it is a-z, we add the freq
            sum += word_frequency(item, 'en')
            if re.match('^[a-z]+$', item):
                count = count+1
                filehandle.writelines("%s\n" % item)
                if sum > prob:
                    break

    print (sum, count)

def generate_top_n(n):
    topWords = top_n_list('en', n*2, wordlist="large")
    count = 0
    with open('top' + str(n) + '.txt', 'w', encoding='utf-8') as filehandle:
        for word in topWords:
            word = word.lower()
            if re.match('^[a-z]+$', word):
                filehandle.writelines("%s\n" % word)
                count = count + 1
                if count == n:
                    break

    print ('top' + str(n) + '.txt')

def get_sum_freq(count):
    sum = 0
    topWords = top_n_list('en', count, wordlist="large")
    for word in topWords:
        sum += word_frequency(word, 'en')

    print(sum, count)


get_freq_until(0.9)

get_freq_until(0.95)

get_freq_until(0.97)

# get_freq_until(0.99)

get_sum_freq(100)

get_sum_freq(1000)

generate_top_n(100)

generate_top_n(1000)