using System;
using System.Threading.Tasks;
using Fluxor;

namespace Myriad.Actions;

public class AnimateEffect : Effect<AnimateAction>
{
    /// <inheritdoc />
    public override Task HandleAsync(AnimateAction action, IDispatcher dispatcher)
    {

        switch (action.StepWithResult.Step)
        {
            //case Step.Move move:
            //    dispatcher.Dispatch(new MoveAction(action.GameId, action.StepWithResult.MoveResult!, move.Coordinate));
            //    break;
            case Step.Rotate rotate:
                dispatcher.Dispatch(new RotateAction(rotate.Amount));
                break;
            case Step.ClearPositionsStep _:
                dispatcher.Dispatch(new Step.ClearPositionsStep());
                break;
            case Step.SetFoundWord sfw :
                dispatcher.Dispatch(new SetPositionsAction(sfw.Word.Path, new AnimationWord(sfw.Word.AnimationString, AnimationWord.WordType.Found)));
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        return Task.CompletedTask;
    }
}