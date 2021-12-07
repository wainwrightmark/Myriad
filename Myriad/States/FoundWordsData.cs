using System.Collections.Immutable;
using System.Linq;

namespace Myriad.States;

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
        (ImmutableSortedDictionary<FoundWord, bool> FoundWordsDictionary) : FoundWordsData
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
            return this with
            {
                FoundWordsDictionary = ImmutableSortedDictionary<FoundWord, bool>.Empty
            };
        }
    }

    public abstract bool WordIsFound(FoundWord s);

    public abstract FoundWordsData FindWord(FoundWord word);

    public abstract FoundWordsData Reset();
}