using System;
using System.Threading.Tasks;
using Fluxor;

namespace Moggle
{

public record AnimateAction(Animation Animation) : IAction<UIState>
{
    /// <inheritdoc />
    public UIState Reduce(UIState state)
    {
        var (animation, _) = Animation.Increment();

        return state with { Animation = animation };
    }
}

public class AnimateEffect : Effect<AnimateAction>
{
    /// <inheritdoc />
    public override async Task HandleAsync(AnimateAction action, IDispatcher dispatcher)
    {
        await Task.CompletedTask;

        switch (action.Animation.Current)
        {
            case Step.Move move:
                dispatcher.Dispatch(new ChooseCellAction(move.Coordinate));
                break;
            case Step.Rotate rotate:
                dispatcher.Dispatch(new RotateAction(rotate.Amount));
                break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
}

}
