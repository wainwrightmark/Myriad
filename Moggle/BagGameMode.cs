using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using MoreLinq.Extensions;

namespace Moggle
{

public abstract record BagGameMode : IMoggleGameMode
{
    public Rune GetRandomRune(Random random) => Letters.EnumerateRunes().Shuffle(random).First();

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public (MoggleBoard board, SolveSettings solveSettings, TimeSituation TimeSituation) CreateGame(
        ImmutableDictionary<string, string> settings)
    {
        var width  = Width.Get(settings);
        var height = Height.Get(settings);
        var seed = Seed.Get(settings);

        var size = width * height;

        ImmutableArray<Letter> array;

        if (string.IsNullOrWhiteSpace(seed))
        {
            var allLetters = new List<Letter>();

            while (allLetters.Count < size)
            {
                allLetters.AddRange(
                    GetDefaultLetters(width, height)
                .EnumerateRunes()
                .Select(Letter.Create)
                .Take(size - allLetters.Count)
                    );
            }

            array = allLetters.ToImmutableArray();
        }
        else
        {
            var allLetters = new List<Letter>();
            var random = RandomHelper.GetRandom(seed);

            while (allLetters.Count < size)
            {
                allLetters.AddRange(
                    Letters.EnumerateRunes()
                        .Shuffle(random)
                        .Take(size - allLetters.Count)
                        .Select(Letter.Create)
                );
            }

            array = allLetters.Shuffle(random)
                .ToImmutableArray();//shuffle again for to prevent patterns in very large grids
        }

        var board = new MoggleBoard(array, width);

        var solveSettings = GetSolveSettings(settings);

        var ts = TimeSituation.GetFromSettings(TimeSituation.Duration, settings);

        return (board, solveSettings, ts);
    }

    public abstract string Letters { get; }
    public abstract SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings);

    public virtual Setting.Integer Width => new(nameof(Width), 1, int.MaxValue, 4);
    public virtual Setting.Integer Height => new(nameof(Height), 1, int.MaxValue, 4);

    public virtual Setting.Integer DurationSetting => TimeSituation.Duration;

    public virtual Setting.String Seed => new(nameof(Seed), null, "", "Random Seed") { GetRandomValue = GoodSeedHelper.GetGoodSeed };

    public abstract string GetDefaultLetters(int width, int height);

    /// <inheritdoc />
    public virtual IEnumerable<Setting> Settings
    {
        get
        {
            yield return Seed;
            yield return Width;
            yield return Height;
            yield return DurationSetting;
        }
    }
}

}
