using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record RotateAction(bool Clockwise) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state)
    {
        var newRotation = Clockwise ? state.Rotation + 1 : state.Rotation - 1;

        return state with
        {
            Rotation = newRotation,
            ChosenPositions = state.ChosenPositions
                .Select(x => x.Rotate(state.Board.MaxCoordinate, Clockwise ? -1 : 1))
                .ToImmutableList(),
            Board = state.Board with{Columns = state.Board.Rows}
        };
    }
}

}
