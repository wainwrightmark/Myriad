using Moggle.States;

namespace Moggle.Actions
{

public record CheatAction : IAction<CheatState>
{
    /// <inheritdoc />
    public CheatState Reduce(CheatState state)
    {
        if (!state.AllowCheating)
            return state;

        return state with { Revealed = true };
    }
}

}
