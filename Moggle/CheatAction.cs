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

        state = state with { CheatWords = ImmutableList<string>.Empty };
        return state;
    }
}

}