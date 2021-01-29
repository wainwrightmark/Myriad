namespace Moggle
{

public record EnableWord(string Word, bool Enable) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board)
    {
        if (Enable)
            return board with { DisabledWords = board.DisabledWords.Remove(Word) };

        return board with { DisabledWords = board.DisabledWords.Add(Word) };
    }
}

}