using Moggle.States;

namespace Moggle
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

public record EnableCheatingAction : IAction<CheatState>
{
    /// <inheritdoc />
    public CheatState Reduce(CheatState state)
    {
        return state with { AllowCheating = true };
    }
}

}
