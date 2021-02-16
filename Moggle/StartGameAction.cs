namespace Moggle
{

public record StartGameAction(
    string Seed,
    int Width,
    int Height,
    bool Classic,
    int Duration,
    bool AllowCheating,
    int MinWordLength) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board) => board.StartNewGame(
        Seed,
        Width,
        Height,
        Classic,
        Duration,
        AllowCheating,
        MinWordLength
    );
}

}
