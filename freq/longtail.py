# top_n_list(lang, n, wordlist='best')

from wordfreq import *
import re

top10k = top_n_list('en', 1000000, wordlist="large")
print(len(top10k))

top40k =top_n_list('en', 40145, wordlist="large")
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
            sum += word_frequency(item, 'en')
            count = count+1
            filehandle.writelines("%s\n" % item)
            if sum > prob:
                break

    print (sum, count)

get_freq_until(0.9)

get_freq_until(0.95)

get_freq_until(0.97)

get_freq_until(0.99)