using System;

namespace Moggle
{

public record Coordinate(int Row, int Column)
{
    public Coordinate Rotate(
        int size,
        int rotation)
    {
        var realSize = size - 1;

        while (rotation < 0)
            rotation += 4;

        return (rotation % 4) switch
        {
            0 => this,
            1 => new(realSize - Column, Row),
            2 => new(realSize - Row, realSize - Column),
            3 => new(Column, realSize - Row),
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
}

}
