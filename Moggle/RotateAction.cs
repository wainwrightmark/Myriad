using Moggle.States;

namespace Moggle
{

public record RotateAction(int Amount) : IAction<RecentWordsState>
{

    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        return state with { Rotation = (state.Rotation + Amount) % 4 };
    }
}


}
