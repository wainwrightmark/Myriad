using Moggle.States;

namespace Moggle.Actions
{

public record RotateAction(int Amount) : IAction<RecentWordsState>
{

    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        return state with { Rotation = (state.Rotation + Amount) % 4 };
    }
}

public record FlipAction : IAction<RecentWordsState>
{
    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        return state with { Flip = !state.Flip };
    }
}



}
