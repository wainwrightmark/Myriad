namespace Moggle
{

public record StartGameAction(string Seed, int Duration) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board)
    {
        return board.StartNewGame(Seed, Duration);
    }
}

}