using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;

namespace Moggle.States
{



public record ChosenPositionsState(ImmutableList<Coordinate> ChosenPositions)
{
    public string GetPathData(
        MoggleBoard board,
        int rotate,
        bool flip,
        double fullWidth,
        double fullHeight)
    {
        var coordinates = GetPathCoordinates(board, rotate, flip, fullWidth, fullHeight)
            .ToList();

        var d = "M " + coordinates.Select(x => $"{x.x:N2} {x.y:N2}").ToDelimitedString(" L ");

        return d;
    }

    private IEnumerable<(double x, double y)> GetPathCoordinates(
        MoggleBoard board,
        int rotate,
        bool flip,
        double fullWidth,
        double fullHeight)
    {
        var squareSize = Math.Min(fullWidth, fullHeight) / Math.Max(board.Columns, board.Rows);

        if (ChosenPositions.Any())
        {
            var locations = ChosenPositions.Select(GetLocation).ToArray();

            for (var i = 0; i < board.Letters.Length; i++)
            {
                var index = Math.DivRem(
                    i * ChosenPositions.Count,
                    board.Letters.Length,
                    out var remainder
                );

                var loc = locations[index];

                if (remainder == 0 || locations.Length <= index + 1)
                    yield return loc;
                else
                {
                    var next = locations[index + 1];

                    //TODO slight curve
                    var inbetween = (
                        GetInbetween(loc.x, next.x, remainder, board.Letters.Length),
                        GetInbetween(loc.y, next.y, remainder, board.Letters.Length));

                    yield return inbetween;
                }
            }
        }
        else
        {
            var centre = (fullWidth / 2, fullHeight / 2);

            for (var i = 0; i < board.Letters.Length; i++)
            {
                yield return centre;
            }
        }

        static double GetInbetween(double d1, double d2, int numerator, int denominator)
        {
            var t = d2 * numerator + d1 * (denominator - numerator);
            return t / denominator;
        }

        (double x, double y) GetLocation(Coordinate coordinate)
        {
            var rotated = GetRotated(coordinate);

            var cx = (rotated.Column + 0.5) * squareSize;
            var cy = (rotated.Row + 0.5) * squareSize;

            return (cx, cy);
        }

        Coordinate GetRotated(Coordinate coordinate) => coordinate.RotateAndFlip(
            board.MaxCoordinate,
            rotate * -1,
            flip
        );
    }
}

}
