using Moggle.Actions;
using Moggle.States;

namespace Moggle
{

public record EnableDisableWordAction(FoundWord Word, bool Enable) : IAction<FoundWordsState>
{
    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState state)
    {
        return state.EnableWord(Word, Enable);
    }
}

}
