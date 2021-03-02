using System;
using System.Threading.Tasks;
using Fluxor;

namespace Moggle.Actions
{

public class AnimateEffect : Effect<AnimateAction>
{
    /// <inheritdoc />
    public override Task HandleAsync(AnimateAction action, IDispatcher dispatcher)
    {

        switch (action.StepWithResult.Step)
        {
            case Step.Move move:
                dispatcher.Dispatch(new MoveAction(action.GameId, action.StepWithResult.MoveResult!, move.Coordinate));
                break;
            case Step.Rotate rotate:
                dispatcher.Dispatch(new RotateAction(rotate.Amount));
                break;
            case Step.SetCoordinatesAction setCoordinates:
                dispatcher.Dispatch(new Step.SetCoordinatesAction(setCoordinates.Path));
                break;
            default:                                throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }
}

}
