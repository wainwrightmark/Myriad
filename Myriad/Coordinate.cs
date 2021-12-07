using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Myriad;

public record Coordinate(int Row, int Column) : IComparable<Coordinate>, IComparable
{
    public Coordinate RotateAndFlip(
        Coordinate maxCoordinate,
        int rotation,
        bool flip)
    {
        while (rotation < 0)
            rotation += 4;

        var max = Math.Max(maxCoordinate.Column, maxCoordinate.Row);

        var rotated = (rotation % 4) switch
        {
            0 => this,
            1 => new((max - Column), Row),
            2 => new((max - Row), (max - Column)),
            3 => new(Column, (max - Row)),
            _ => throw new ArgumentException(nameof(rotation))
        };

        if (flip)
            return rotated.ReflectColumn(maxCoordinate.Column);

        return rotated;
    }

    public static (int rotate, bool flip)? GetTransform(
        Coordinate source,
        Coordinate target,
        Coordinate maxCoordinate)
    {
        foreach (var flip in new[] { false, true })
        {
            for (var i = 0; i < 4; i++)
            {
                var r = source.RotateAndFlip(maxCoordinate, i, flip);

                if (r == target)
                    return (i, flip);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the angle in degrees from this coordinate to the other
    /// </summary>
    public double GetAngle(Coordinate other)
    {
        float xDiff = other.Column - Column;
        float yDiff = other.Row - Row;
        return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
    }

    public Coordinate ReflectColumn(int maxColumn) => new(Row, maxColumn - Column);
    public Coordinate ReflectRow(int maxRow) => new(maxRow - Row, Column);

    public bool IsAdjacent(Coordinate co)
    {
        if (Row == co.Row && Column == co.Column)
            return false;

        var rowDiff = Math.Abs(Row - co.Row);
        var colDiff = Math.Abs(Column - co.Column);

        return rowDiff <= 1 && colDiff <= 1;
    }

    private static readonly List<(int rowModifier, int colModifier)> AdjacentPositions =
        new()
        {
            (-1, -1),
            (-1, 0),
            (-1, 1),
            (0, -1),
            (0, 1),
            (1, -1),
            (1, 0),
            (1, 1)
        };

    public IEnumerable<Coordinate> GetAdjacentCoordinates(Coordinate maxCoordinate)
    {
        foreach (var (rowModifier, colModifier) in AdjacentPositions)
        {
            var newR = Row + rowModifier;
            var newC = Column + colModifier;

            if (newR < 0 || newR > maxCoordinate.Row)
                continue;

            if (newC < 0 || newC > maxCoordinate.Column)
                continue;

            yield return new Coordinate(newR, newC);
        }
    }

    public bool HasAtLeastXNeighbors(int x, Coordinate maxCoordinate)
    {
        int requiredDimensions;

        switch (x)
        {
            case <= 0: return true;
            case <= 1:
                requiredDimensions = 1;
                break;
            case <= 3:
                requiredDimensions = 2;
                break;
            case <= 5:
                requiredDimensions = 3;
                break;
            case <= 8:
                requiredDimensions = 4;
                break;
            default: return false;
        }

        var dimensions = 0;

        if (Row > 0)
            dimensions++;

        if (Row < maxCoordinate.Row)
            dimensions++;

        if (Column > 0)
            dimensions++;

        if (Column < maxCoordinate.Column)
            dimensions++;

        return dimensions >= requiredDimensions;
    }

    public IEnumerable<Coordinate> GetPositionsUpTo()
    {
        for (var r = 0; r <= Row; r++)
        for (var c = 0; c <= Column; c++)
            yield return new Coordinate(r, c);
    }

    public int DistanceFromCentre(Coordinate maxCoordinate)
    {
        var dRow = Row * 2;
        var dCol = Column * 2;

        var rDist = Math.Abs(maxCoordinate.Row - dRow);
        var cDist = Math.Abs(maxCoordinate.Column - dCol);

        return rDist + cDist;
    }

    /// <inheritdoc />
    public override string ToString() => $"({Row},{Column})";

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is Coordinate c)
            return CompareTo(c);

        return 0;
    }

    public static Coordinate GetMaxCoordinateForSquareGrid(int numberOfNodes)
    {
        var root = Math.Sqrt(numberOfNodes);

        var ceiling = (int)Math.Ceiling(root);

        return new Coordinate(ceiling - 1, ceiling - 1);
    }

    /// <inheritdoc />
    public int CompareTo(Coordinate? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        var rowComparison = Row.CompareTo(other.Row);

        if (rowComparison != 0)
            return rowComparison;

        return Column.CompareTo(other.Column);
    }

    private static readonly Regex _regex = new(
        @"\A\s*\(?(?<x>\d+)\s*,\s*(?<y>\d+)\)?\s*\Z",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public static Coordinate? TryParse(string s)
    {
        var m = _regex.Match(s);

        if (m.Success)
            return new Coordinate(int.Parse(m.Groups["x"].Value), int.Parse(m.Groups["y"].Value));

        return null;
    }
}