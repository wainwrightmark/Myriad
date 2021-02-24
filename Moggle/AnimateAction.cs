using System;
using System.Threading.Tasks;
using Fluxor;
using Moggle.States;

namespace Moggle
{

public record AnimateAction(StepWithResult StepWithResult, string GameString) : IAction<AnimationState>
{
    /// <inheritdoc />
    public AnimationState Reduce(AnimationState state)
    {

        return state with { AnimationFrame = StepWithResult.NewIndex};
    }
}

public class AnimateEffect : Effect<AnimateAction>
{
    /// <inheritdoc />
    public override Task HandleAsync(AnimateAction action, IDispatcher dispatcher)
    {

        switch (action.StepWithResult.Step)
        {
            case Step.Move move:
                dispatcher.Dispatch(new MoveAction(action.StepWithResult.MoveResult!, move.Coordinate, action.GameString));
                break;
            case Step.Rotate rotate:
                dispatcher.Dispatch(new RotateAction(rotate.Amount));
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }
}



}
