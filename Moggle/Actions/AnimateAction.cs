using Moggle.States;

namespace Moggle.Actions
{

public record AnimateAction(string GameId, StepWithResult StepWithResult) : IAction<AnimationState>
{
    /// <inheritdoc />
    public AnimationState Reduce(AnimationState state)
    {

        return state with { AnimationFrame = StepWithResult.NewIndex};
    }
}

}
