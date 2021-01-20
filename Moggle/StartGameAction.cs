namespace Moggle
{

public record StartGameAction(string Seed, bool Classic, int Duration) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board)
    {
        return board.StartNewGame(Seed, Classic, Duration);
    }
}

}
