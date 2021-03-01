using Moggle.Actions;
using Moggle.States;

namespace Moggle
{

public record UnCheckWord(FoundWord Word, bool Enable) : IAction<FoundWordsState>
{
    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState board)
    {
        if (Enable)
            return board with { UncheckedWords = board.UncheckedWords.Remove(Word) };

        return board with { UncheckedWords = board.UncheckedWords.Add(Word) };
    }
}

}
