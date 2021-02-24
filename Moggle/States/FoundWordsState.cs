using System.Collections.Immutable;
using System.Linq;

namespace Moggle.States
{

public record FoundWordsState(ImmutableHashSet<FoundWord> UncheckedWords)
{
    public int GetNumberOfWords(ImmutableSortedSet<FoundWord> foundWords)
    {
        return foundWords.Except(UncheckedWords).Count;
    }

    public int GetScore(ImmutableSortedSet<FoundWord> foundWords)
    {
        return foundWords.Except(UncheckedWords).Sum(x => x.Points);
    }
}

}