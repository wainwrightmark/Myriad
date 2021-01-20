namespace Moggle
{

public record ChooseCellAction(Coordinate Coordinate) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state) => state.TryGetMoveResult(Coordinate) ?? state;
}

}
