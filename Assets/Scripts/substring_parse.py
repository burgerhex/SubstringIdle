from itertools import product
from tqdm import tqdm

alphabet = [chr(ord('a') + i) for i in range(26)]
all_words = set()

with open("../words_alpha.txt", 'r') as file:
    for line in file.readlines():
        all_words.add(line.lower())

substring_occurrences = {}

for letter_pair in tqdm(list(product(alphabet, repeat=2))):
    substr = "".join(letter_pair)
    substring_occurrences[substr] = 0
    for word in all_words:
        if substr in word:
            substring_occurrences[substr] += 1

substring_occurrences_sorted = list(substring_occurrences.items())
substring_occurrences_sorted.sort(key=lambda p: -p[1])

with open("sorted_occurrences.txt", 'w') as file:
    for word, occurrences in substring_occurrences_sorted:
        file.write(word + ": " + str(occurrences) + "\n")
