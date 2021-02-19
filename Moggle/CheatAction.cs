using System.Collections.Immutable;

namespace Moggle
{

public record CheatAction : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state)
    {
        if (state.CheatWords != null)
            return state;

        var cheatWords = state.Solver.GetPossibleSolutions(state.Board);

        state = state with { CheatWords = cheatWords.ToImmutableList() };
        return state;
    }
}

}
