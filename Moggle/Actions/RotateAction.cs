using Moggle.States;

namespace Moggle.Actions
{

public record RotateAction(int Amount) : IAction<RecentWordsState>
{

    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        var amount = Amount;
        if (state.Flip)
            amount *= -1;
        return state with { Rotation = (state.Rotation + amount) % 4 };
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
