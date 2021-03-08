using System.Collections.Immutable;
using Moggle.States;

namespace Moggle.Actions
{

public record CheatAction(Solver Solver, MoggleBoard Board) : IAction<CheatState>,
                                                              IAction<FoundWordsState>
{
    /// <inheritdoc />
    public CheatState Reduce(CheatState state)
    {
        if (!state.AllowCheating)
            return state;

        return state with {Revealed = false};
    }

    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState state)
    {
        var possibleWords = Solver.GetPossibleSolutions(Board).ToImmutableList();

        state = state.FindWords(possibleWords);

        return state;
    }
}

}
