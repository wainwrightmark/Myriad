﻿namespace Moggle
{

public record StartGameAction(string Seed, int Width, int Height, bool Classic, int Duration) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board) => board.StartNewGame(Seed, Width, Height, Classic, Duration);
}

}