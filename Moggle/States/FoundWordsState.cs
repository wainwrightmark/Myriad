using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle.States
{

public abstract record FoundWordsData
{
    public record TargetWordsData
    (
        ImmutableDictionary<string, (string group, FoundWord? word)>
            WordsToFind) : FoundWordsData
    {
        /// <inheritdoc />
        public override bool WordIsFound(FoundWord s)
        {
            return WordsToFind.TryGetValue(s.Comparison, out var p) && p.word is not null;
        }

        /// <inheritdoc />
        public override FoundWordsData FindWord(FoundWord word)
        {
            if (WordsToFind.TryGetValue(word.Comparison, out var g))
            {
                var newWordsToFind = WordsToFind.SetItem(word.Comparison, (g.group, word));

                return this with { WordsToFind = newWordsToFind };
            }

            return this;
        }
    }

    public record OpenSearchData
        (ImmutableDictionary<FoundWord, bool> FoundWordsDictionary) : FoundWordsData
    {
        public int GetNumberOfWords()
        {
            return FoundWordsDictionary.Count(x => x.Value);
        }

        public int GetScore()
        {
            return FoundWordsDictionary.Where(x => x.Value).Sum(x => x.Key.Points);
        }

        /// <inheritdoc />
        public override bool WordIsFound(FoundWord s)
        {
            return FoundWordsDictionary.ContainsKey(s);
        }

        /// <inheritdoc />
        public override FoundWordsData FindWord(FoundWord word)
        {
            if (FoundWordsDictionary.ContainsKey(word))
                return this;

            return this with { FoundWordsDictionary = FoundWordsDictionary.Add(word, true) };
        }
    }

    public abstract bool WordIsFound(FoundWord s);

    public abstract FoundWordsData FindWord(FoundWord word);
}

public record FoundWordsState(FoundWordsData Data)
{
    public FoundWordsState EnableWord(FoundWord word, bool enable)
    {
        if (Data is FoundWordsData.OpenSearchData osd)
        {
            osd = osd with
            {
                FoundWordsDictionary = osd.FoundWordsDictionary.SetItem(word, enable)
            };

            return this with { Data = osd };
        }

        return this;
    }

    public FoundWordsState FindWord(FoundWord word)
    {
        return this with { Data = Data.FindWord(word) };
    }

    public FoundWordsState FindWords(IEnumerable<FoundWord> words)
    {
        var data = this.Data;

        foreach (var foundWord in words)
        {
            data = data.FindWord(foundWord);
        }

        return this with { Data = data };
    }
}

}
