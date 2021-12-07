using System;
using Myriad.States;

namespace Myriad.Actions;

public record AnimateAction(string GameId, StepWithResult StepWithResult) : IAction<AnimationState>
{
    /// <inheritdoc />
    public AnimationState Reduce(AnimationState state)
    {

        return state with { LastFrame = DateTime.Now, AnimationFrame = StepWithResult.NewIndex};
    }
}