using Myriad.Actions;
using Myriad.States;

namespace Myriad
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
