using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle.States
{

public record TargetWordContext
{
    public TargetWordContext(IEnumerable<TargetWord> targetWords)
    {
        Dictionary  = targetWords.ToDictionary(x => x.Comparison);
        GroupLookup = Dictionary.Values.ToLookup(x => x.Group);
    }

    public IReadOnlyDictionary<string, TargetWord> Dictionary { get; }
    public ILookup<string, TargetWord> GroupLookup { get; }
}

public record FoundWordsState(
    ImmutableSortedSet<FoundWord> FoundWords,
    ImmutableHashSet<string> FWComparisonStrings,
    ImmutableHashSet<FoundWord> UncheckedWords,
    TargetWordContext? TargetWordContext)
{
    public int RemainingInGroup(string groupKey)
    {
        if (TargetWordContext is null)
            return 0;

        var c = TargetWordContext.GroupLookup[groupKey]
            .Count(x => !Contains(x));

        return c;
    }

    public bool Contains(FoundWord fw)
    {
        var r = FWComparisonStrings.Contains(fw.Comparison);
        //var r = FoundWords.Contains(fw);
        return r;
    }

    public int GetNumberOfWords()
    {
        return FoundWords.Except(UncheckedWords).Count;
    }

    public int GetScore()
    {
        return FoundWords.Except(UncheckedWords).Sum(x => x.Points);
    }
}

}
