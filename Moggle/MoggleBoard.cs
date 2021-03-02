using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using MoreLinq;

namespace Moggle
{

public record MoggleBoard
{
    public MoggleBoard(ImmutableArray<Letter> letters, int columns)
    {
        Letters   = letters;
        Columns   = columns;
        UniqueKey = GetUniqueKey();
    }

    public ImmutableArray<Letter> Letters { get; }
    public int Columns { get; }
    public string UniqueKey { get; }


        private string GetUniqueKey()
        {
            if (Columns != Rows)
                return $"{Columns}_{string.Join("", Letters.Select(x => x.WordText))}";

            var options = new List<string>();

            for (var rotation = 0; rotation < 4; rotation++)
            {
                for (var reflection = 0; reflection < 2; reflection++)
                {
                    var s = GetAllCoordinates()
                        .Select(x => x.RotateAndFlip(MaxCoordinate,  rotation, reflection == 0))
                        .Select(GetLetterAtCoordinate)
                        .Select(x => x.WordText)
                        .ToDelimitedString("");
                    options.Add(s);
                }
            }

            return options.OrderBy(x => x).First();
        }

        public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var index = (coordinate.Row * Columns) + coordinate.Column;

        return GetLetterAtIndex(index);
    }

    public Letter GetLetterAtIndex(int i) => Letters[i % Letters.Length];

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

    public ImmutableList<Coordinate>? TryFindWord(string word)
    {
        var letters =
            ImmutableStack.CreateRange(word.EnumerateRunes().Select(Letter.Create).Reverse());

        var list = FindAllPathsToWord(letters, ImmutableList<Coordinate>.Empty, this)
            .FirstOrDefault();

        return list;

        IEnumerable<ImmutableList<Coordinate>> FindAllPathsToWord(
            ImmutableStack<Letter> remainingLetters,
            ImmutableList<Coordinate> usedCoordinates,
            MoggleBoard board)
        {
            if (!remainingLetters.Any())
                yield return usedCoordinates;
            else
            {
                var newRemainingLetters = remainingLetters.Pop(out var letterToFind);

                var last = usedCoordinates.LastOrDefault();

                var coordinatesToCheck = last?.GetAdjacentCoordinates(board.MaxCoordinate) ??
                                         board.GetAllCoordinates();

                foreach (var adj in coordinatesToCheck.Except(usedCoordinates))
                {
                    if (!board.GetLetterAtCoordinate(adj).Equals(letterToFind))
                        continue;

                    var newCoordinates = usedCoordinates.Add(adj);

                    foreach (var finalList in FindAllPathsToWord(
                        newRemainingLetters,
                        newCoordinates,
                        board
                    ))
                        yield return finalList;
                }
            }
        }
    }
}

}
