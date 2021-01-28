using System;
using System.Collections.Generic;

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
            1 => new((max - Column), Row),
            2 => new((max - Row), (max - Column)),
            3 => new(Column, (max - Row)),
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

    private static readonly List<(int rowModifier, int colModifier)> AdjacentPositions =
        new() {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),  (0, 1),
            (1, -1), (1,0), (1, 1) };

    public IEnumerable<Coordinate> GetAdjacentCoordinates(Coordinate maxCoordinate)
    {
        foreach (var (rowModifier, colModifier) in AdjacentPositions)
        {
            var newR = Row + rowModifier;
            var newC = Column + colModifier;

            if(newR < 0 || newR > maxCoordinate.Row)
                continue;
            if(newC < 0 || newC > maxCoordinate.Column)
                continue;

            yield return new Coordinate(newR, newC);
        }
    }

    /// <inheritdoc />
    public override string ToString() => $"({Row},{Column})";
}

}
