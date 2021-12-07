using System.Collections.Generic;
using System.Linq;
using Myriad.States;

namespace Myriad.Actions;

public record LoadWordsAction(IReadOnlyList<WordCheckResult.Legal> Save) : IAction<FoundWordsState>
{
    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState state)
    {
        var r = state.FindWords(Save.Select(x => x.Word));

        return r;
    }
}