using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Moggle
{

public record WordList(IReadOnlySet<string> LegalWords, IReadOnlySet<string> LegalPrefixes)
{
    public static WordList FromWords(IEnumerable<string> words)
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine("Loading words from word list");
        var legalWords = words.ToHashSet(StringComparer.OrdinalIgnoreCase);
        Console.WriteLine($"{legalWords.Count} words found {sw.ElapsedMilliseconds}ms");

        var legalPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var legalWord in legalWords)
            AddAllPrefixes(legalWord, legalPrefixes);

        Console.WriteLine($"{legalPrefixes.Count} prefixes found {sw.ElapsedMilliseconds}ms");

        return new WordList(legalWords, legalPrefixes);
    }

    public static Lazy<WordList> LazyInstance { get; } = new(FromResourceFile);

    public static WordList FromResourceFile()
    {
        var words = Words.WordList.Split('\n');

        return FromWords(words);
    }

    private static void AddAllPrefixes(string s, ISet<string> set)
    {
        for (var length = s.Length - 1; length >= 1; length--)
        {
            var substring = s.Substring(0, length);

            if (!set.Add(substring))
                return; //We already have this prefix and therefore all prior prefixes
        }
    }

    public static WordList Empty { get; } = FromWords(Array.Empty<string>());

    public WordList AddWords(IReadOnlyCollection<string> words)
    {
        var newWords    = LegalWords.Union(words);
        var newPrefixes = LegalPrefixes.ToHashSet();

        foreach (var word in words)
            AddAllPrefixes(word, newPrefixes); return new WordList(newWords.ToHashSet(), newPrefixes);
    }

}

}
