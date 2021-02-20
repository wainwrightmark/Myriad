namespace Moggle
{

public record ChooseCellAction(Coordinate Coordinate) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state)
    {
        var moveResult = state.TryGetMoveResult(Coordinate);

        if (moveResult is MoveResult.SuccessResult sr)
            return sr.MoggleState;

        return state;
    }
}

}
