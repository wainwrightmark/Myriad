using Myriad.States;

namespace Myriad.Actions
{

public record ResetFoundWordsAction(Board Board) : IAction<FoundWordsState>
{
    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState state)
    {
        return state.Reset();
    }
}

}
