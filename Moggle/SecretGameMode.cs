using System.Collections.Generic;
using System.Collections.Immutable;

namespace Moggle
{

public record SecretGameMode : IMoggleGameMode
{
    private SecretGameMode() { }
    public static SecretGameMode Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Secret";

    /// <inheritdoc />
    public (MoggleBoard board, SolveSettings solveSettings, TimeSituation TimeSituation) CreateGame(
        ImmutableDictionary<string, string> settings)
    {
        var words = Words.Get(settings);

        var minWordLength = MinWordLength.Get(settings);

        var grid = Creator.GridCreator.CreateNodeGridFromText(words, null, 10000);

        var random = RandomHelper.GetRandom(words);

        var board         = grid.ToMoggleBoard(() => ModernGameMode.Instance.GetRandomRune(random));
        var solveSettings = new SolveSettings(minWordLength, false, null);

        var ts = TimeSituation.Create(TimeSituation.Duration.Get(settings));

        return (board, solveSettings, ts);
    }

    public static readonly Setting.String Words =
        new(
            nameof(Words),
            @"\A(?:\w+)(?:\s\w+)*\Z",
            "horizontal dancing",
            "Words to appear in the grid"
        ) { GetRandomValue = GoodSeedHelper.GetGoodSeed };

    public static readonly Setting.Integer MinWordLength = new(nameof(MinWordLength), 2, 8, 3);

    /// <inheritdoc />
    public IEnumerable<Setting> Settings
    {
        get
        {
            yield return MinWordLength;
            yield return Words;
            yield return TimeSituation.Duration;
        }
    }
}

}
