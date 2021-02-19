using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record FixedGameMode : IMoggleGameMode
{
    private FixedGameMode() { }
    public static FixedGameMode Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Fixed";

    /// <inheritdoc />
    public (MoggleBoard board, SolveSettings solveSettings) CreateGame(
        ImmutableDictionary<string, string> settings)
    {
        var letters = Letters.Get(settings)
            .EnumerateRunes()
            .Select(Letter.Create)
            .ToImmutableArray();

        var c = Coordinate.GetMaxCoordinateForSquareGrid(letters.Length);

        var total = (c.Column + 1) * (c.Row + 1);

        if (letters.Length < total)
        {
            letters = letters.AddRange(Enumerable.Repeat(PaddingLetter, total - letters.Length));
        }

        var board = new MoggleBoard(letters, c.Column + 1);

        int? minWordLength = MinWordLength.Get(settings);

        if (minWordLength < 0)
            minWordLength = null;

        int? target = Target.Get(settings);

        if (target < 0)
            target = null;

        var allowEquations = letters.Any(x => x.WordText.Equals("="));

        var solveSettings = new SolveSettings(minWordLength, allowEquations, target);

        return (board, solveSettings);
    }

    private static readonly Letter PaddingLetter = Letter.Create("😊".EnumerateRunes().Single());

    public static readonly Setting.String Letters =
        new(nameof(Letters), @".+", "tarantula", "Exact Grid Letters") { GetRandomValue = GoodSeedHelper.GetGoodSeed };

    public static readonly Setting.Integer MinWordLength = new(nameof(MinWordLength), -1, 8, 3);

    public static readonly Setting.Integer Target = new(nameof(Target), -1, int.MaxValue, -1);

    /// <inheritdoc />
    public IEnumerable<Setting> Settings
    {
        get
        {
            yield return Letters;
            yield return MinWordLength;
            yield return Target;
        }
    }
}

}