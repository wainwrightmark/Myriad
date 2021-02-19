using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Moggle
{

public record MoggleBoard(ImmutableArray<Letter> Letters, int Width)
{
    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var index = (coordinate.Row * Width) + coordinate.Column;

        return GetLetterAtIndex(index);
    }

    public Letter GetLetterAtIndex(int i)
    {
        return Letters[i % Letters.Length];
    }

    public int Height => Letters.Length / Width;

    public Coordinate MaxCoordinate => new(Height - 1, Width - 1);

    public IEnumerable<Coordinate> GetAllCoordinates()
    {
        for (var c = 0; c < Width; c++)
        for (var r = 0; r < Height; r++)
            yield return new Coordinate(r, c);
    }

    public string ToMultiLineString()
    {
        StringBuilder sb = new();

        for (var r = 0; r < Height; r++)
        {
            for (var c = 0; c < Width; c++)
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
