using System;
using System.Collections.Generic;
using System.IO;
using Random = UnityEngine.Random;

public class WordManager
{
    private const string WordsFile = "Assets/words_alpha.txt";
    private const string OccurrencesFile = "Assets/sorted_occurrences.txt";
    private const int StoredSubstrings = 10;
    private const int MinWordsPerSubstring = 1000;

    private readonly HashSet<string> _words = new HashSet<string>();
    private readonly List<string> _substrings = new List<string>();
    private readonly Dictionary<string, int> _substringOccurrences = new Dictionary<string, int>();
    private readonly Queue<string> _lastUsedSubstringsQueue = new Queue<string>();
    private readonly HashSet<string> _lastUsedSubstringsSet = new HashSet<string>();

    public WordManager()
    {
        ReadWordsFile();
        ReadOccurrencesFile();
    }

    private void ReadWordsFile()
    {
        StreamReader reader = new StreamReader(WordsFile);
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            _words.Add(line);
        }
        
        reader.Close();
    }

    private void ReadOccurrencesFile()
    {
        StreamReader reader = new StreamReader(OccurrencesFile);
        string line;
        
        while ((line = reader.ReadLine()) != null)
        {
            string[] parts = line.Split(new[] {": "}, StringSplitOptions.None);
            string letters = parts[0];
            int occurrences = int.Parse(parts[1]);
            _substrings.Add(letters);
            _substringOccurrences[letters] = occurrences;
        }
        
        reader.Close();
    }

    public bool WordExists(string word)
    { 
        return _words.Contains(word);
    }

    public string NextSubstring()
    {
        string substr;
        do
        {
            int index = Random.Range(0, _substrings.Count);
            substr = _substrings[index];
        } while (_substringOccurrences[substr] < MinWordsPerSubstring || _lastUsedSubstringsSet.Contains(substr));

        _lastUsedSubstringsSet.Add(substr);
        _lastUsedSubstringsQueue.Enqueue(substr);

        if (_lastUsedSubstringsQueue.Count > StoredSubstrings)
        {
            string nowAllowed = _lastUsedSubstringsQueue.Dequeue();
            _lastUsedSubstringsSet.Remove(nowAllowed);
        }

        return substr;
    }
}