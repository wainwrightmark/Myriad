using Moggle.States;

namespace Moggle.Actions
{

public record ResetFoundWordsAction(MoggleBoard Board) : IAction<FoundWordsState>
{
    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState state)
    {
        return state.Reset();
    }
}

}
