using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record RotateAction(bool Clockwise) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board)
    {
        var newRotation = Clockwise ? board.Rotation + 1 : board.Rotation - 1;

        return board with
        {
            Rotation = newRotation,
            ChosenPositions = board.ChosenPositions
                .Select(x => x.Rotate(board.Board.ColumnCount, Clockwise ? -1 : 1))
                .ToImmutableList()
        };
    }
}

}
