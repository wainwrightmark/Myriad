using Moggle.States;

namespace Moggle.Actions
{

public record EnableCheatingAction : IAction<CheatState>
{
    /// <inheritdoc />
    public CheatState Reduce(CheatState state)
    {
        return state with { AllowCheating = true };
    }
}

}