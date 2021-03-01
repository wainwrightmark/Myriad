using System.Collections.Immutable;
using System.Linq;

namespace Moggle.States
{

public record FoundWordsState(
    ImmutableSortedSet<FoundWord> FoundWords,
    ImmutableHashSet<FoundWord> UncheckedWords)
{
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
