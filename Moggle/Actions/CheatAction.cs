using System.Collections.Immutable;
using Moggle.States;

namespace Moggle.Actions
{

public record CheatAction(Solver Solver, MoggleBoard Board) : IAction<CheatState>
{
    /// <inheritdoc />
    public CheatState Reduce(CheatState state)
    {
        if (!state.AllowCheating)
            return state;

        var possibleWords = Solver.GetPossibleSolutions(Board).ToImmutableList();

        return state with { Revealed = true, PossibleWords = possibleWords};
    }
}

}
