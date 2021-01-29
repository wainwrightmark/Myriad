using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record MoggleBoard(
    ImmutableArray<BoggleDice> Dice,
    ImmutableList<DicePosition> Positions,
    int Width)
{
    private static ImmutableArray<BoggleDice> ClassicDice = ImmutableArray.Create<BoggleDice>(
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
    );

    private static ImmutableArray<BoggleDice> ModernDice = ImmutableArray.Create<BoggleDice>(
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
    );

    public static MoggleBoard CreateFromString(string s)
    {
        int width;
        int height;

        switch (s.Length)
        {
            case <= 9:
            {
                width  = 3;
                height = 3;
                break;
            }
            case <= 16:
            {
                width  = 4;
                height = 4;
                break;
            }

            case <= 25:
            {
                width  = 5;
                height = 5;
                break;
            }
            case <= 36:
            {
                width  = 6;
                height = 6;
                break;
            }
            default: throw new ArgumentException("letters");
        }

        var letters   = s.PadRight(height * width, '*');
        var dice      = letters.Select(x => new BoggleDice(new string(x, 6))).ToImmutableArray();
        var positions = Enumerable.Range(0, letters.Length).Select(x => new DicePosition(x, 0)).ToImmutableList();
        return new MoggleBoard(dice, positions, width);
    }

    public static MoggleBoard Create(bool classic, int width, int height) => new(
        classic ? ClassicDice : ModernDice,
        Enumerable.Range(0, width * height).Select(x => new DicePosition(x, 0)).ToImmutableList(),
        width
    );

    public MoggleBoard Randomize(string seed)
    {
        if (string.IsNullOrWhiteSpace(seed))
            return this with
            {
                Positions = Positions.OrderBy(x => x.DiceIndex)
                    .Select(x => x with { FaceIndex = 0 })
                    .ToImmutableList()
            };

        seed = seed.Trim().ToLowerInvariant();

        var code = GetNumber(seed);

        Console.WriteLine(code);
        return Randomize(new Random(code));
    }

    private static int GetNumber(string s)
    {
        var current = 1;

        foreach (var v in s)
        {
            current += v;
            current *= v;
        }

        return current;
    }

    public MoggleBoard Randomize(Random random)
    {
        var newPositions = Positions
            .Select(pos => (pos, random.Next(1000)))
            .OrderBy(x => x.Item2)
            .ThenBy(x => x.pos.DiceIndex)
            .Select(x => x.pos)
            .ToList()
            .Select(x => x.RandomizeFace(random))
            .ToImmutableList();

        return this with { Positions = newPositions };
    }

    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var index = (coordinate.Row * Width) + coordinate.Column;

        return GetLetterAtIndex(index);
    }

    public Letter GetLetterAtIndex(int i)
    {
        var position = Positions[i % Positions.Count];
        var die      = Dice[position.DiceIndex % Dice.Length];
        var letter   = die.Letters[position.FaceIndex % die.Letters.Length];
        return Letter.Create(letter);
    }

    public int Height => Positions.Count / Width;

    public Coordinate MaxCoordinate => new(Height - 1, Width - 1);

    public IEnumerable<Coordinate> GetAllCoordinates()
    {
        for (var c = 0; c < Width; c++)
        for (var r = 0; r < Height; r++)
            yield return new Coordinate(r, c);
    }
}

}
