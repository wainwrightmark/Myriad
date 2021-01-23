using System;

namespace Moggle
{

public record Coordinate(int Row, int Column)
{
    public Coordinate Rotate(
        Coordinate maxCoordinate,
        int rotation)
    {
        while (rotation < 0)
            rotation += 4;

        var max = Math.Max(maxCoordinate.Column, maxCoordinate.Row);

        return (rotation % 4) switch
        {
            0 => this,
            1 => new((max - Column ), Row),
            2 => new((max - Row), (max - Column)),
            3 => new(Column, (max - Row) ),
            _ => throw new ArgumentException(nameof(rotation))
        };
    }

    public bool IsAdjacent(Coordinate co)
    {
        if (Row == co.Row && Column == co.Column)
            return false;

        var rowDiff = Math.Abs(Row - co.Row);
        var colDiff = Math.Abs(Column - co.Column);

        return rowDiff <= 1 && colDiff <= 1;
    }

    /// <inheritdoc />
    public override string ToString() => $"({Row},{Column})";
}

}
