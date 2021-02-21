using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Moggle
{

public record MoggleBoard(ImmutableArray<Letter> Letters, int Columns)
{
    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var index = (coordinate.Row * Columns) + coordinate.Column;

        return GetLetterAtIndex(index);
    }

    public Letter GetLetterAtIndex(int i)
    {
        return Letters[i % Letters.Length];
    }

    public int Rows => Letters.Length / Columns;

    public Coordinate MaxCoordinate => new(Rows - 1, Columns - 1);

    public IEnumerable<Coordinate> GetAllCoordinates()
    {
        for (var c = 0; c < Columns; c++)
        for (var r = 0; r < Rows; r++)
            yield return new Coordinate(r, c);
    }

    public string ToMultiLineString()
    {
        StringBuilder sb = new();

        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                var l = GetLetterAtCoordinate(new Coordinate(r, c));
                sb.Append(l.ButtonText);
            }

            sb.AppendLine();
        }

        return sb.ToString().Trim();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToMultiLineString()
            .Replace('\r',   '\t')
            .Replace('\n',   '\t')
            .Replace("\t\t", "\t")
            .Trim();
    }
}

}
