using System;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;

namespace Moggle
{



public record MoggleBoard(
    ImmutableArray<BoggleDice> Dice,
    ImmutableList<DicePosition> Positions,
    int ColumnCount)
{
    public static readonly MoggleBoard DefaultBoardClassic = new(
        ImmutableArray.Create<BoggleDice>(
            new("AACIOT"),
            new("ABILTY"),
            new("ABJMOQu"),
            new("ACDEMP"),
            new("ACELRS"),
            new("ADENVZ"),
            new("AHMORS"),
            new("BIFORX"),
            new("DENOSW"),
            new("DKNOTU"),
            new("EEFHIY"),
            new("EGKLUY"),
            new("EGINTV"),
            new("EHINPS"),
            new("ELPSTU"),
            new("GILRUW")
        ),
        Enumerable.Range(0, 16).Select(x => new DicePosition(x, 0)).ToImmutableList(),
        4
    );

    public static readonly MoggleBoard DefaultBoardModern = new(
        ImmutableArray.Create<BoggleDice>(
            new("AAEEGN"),
            new("ABBJOO"),
            new("ACHOPS"),
            new("AFFKPS"),
            new("AOOTTW"),
            new("CIMOTU"),
            new("DEILRX"),
            new("DELRVY"),
            new("DISTTY"),
            new("EEGHNW"),
            new("EEINSU"),
            new("EHRTVW"),
            new("EIOSST"),
            new("ELRTTY"),
            new("HIMNUQ"),
            new("HLNNRZ")
        ),
        Enumerable.Range(0, 16).Select(x => new DicePosition(x, 0)).ToImmutableList(),
        4
    );

    public MoggleBoard Randomize(string seed)
    {
        seed = seed.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(seed))
        {
            return this with { Positions = DefaultBoardClassic.Positions };
        }
        else
        {
            return Randomize(new Random(seed.GetHashCode()));
        }
    }

    public MoggleBoard Randomize(Random random)
    {
        var newPositions = Positions.Shuffle(random)
            .Select(x => x.RandomizeFace(random))
            .ToImmutableList();

        return this with { Positions = newPositions };
    }

    public char GetLetterAtCoordinate(Coordinate coordinate)
    {
        var index = (coordinate.Row * ColumnCount) + coordinate.Column;

        return GetLetterAtIndex(index);
    }

    public char GetLetterAtIndex(int i)
    {
        var position = Positions[i % Positions.Count];
        var die      = Dice[position.DiceIndex % Dice.Length];
        var letter   = die.Letters[position.FaceIndex % die.Letters.Length];
        return letter;
    }

    public int RowCount => Positions.Count / ColumnCount;
}

}
