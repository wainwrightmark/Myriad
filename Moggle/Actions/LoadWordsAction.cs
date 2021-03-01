using System.Collections.Generic;
using System.Linq;
using Moggle.States;

namespace Moggle.Actions
{

public record LoadWordsAction(IReadOnlyList<WordCheckResult.Legal> Save) : IAction<FoundWordsState>
{
    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState state)
    {
        var newWords =
            state.FoundWords.Union(
                Save
                    .Select(x => x.Word)
            );

        var newState = state with { FoundWords = newWords };

        return newState;
    }
}

}