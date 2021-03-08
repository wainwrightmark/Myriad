using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle.States
{

public abstract record FoundWordsData
{
    public record TargetWordGroup(string Display, int Order, bool HideText) { }

    public record TargetWordsData
    (
        ImmutableDictionary<string, (TargetWordGroup group, FoundWord? word)>
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

        /// <inheritdoc />
        public override FoundWordsData Reset()
        {
            var newWordsToFind = WordsToFind.ToImmutableDictionary(
                x => x.Key,
                x => (x.Value.group, null as FoundWord)
            );

            return this with { WordsToFind = newWordsToFind };
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

        /// <inheritdoc />
        public override FoundWordsData Reset()
        {
            return this with { FoundWordsDictionary = ImmutableDictionary<FoundWord, bool>.Empty };
        }
    }

    public abstract bool WordIsFound(FoundWord s);

    public abstract FoundWordsData FindWord(FoundWord word);

    public abstract FoundWordsData Reset();
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
        var data = Data;

        foreach (var foundWord in words)
        {
            data = data.FindWord(foundWord);
        }

        return this with { Data = data };
    }

    public FoundWordsState Reset()
    {
        return this with { Data = Data.Reset() };
    }
}

}
