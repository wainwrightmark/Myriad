using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Moggle.States;
using MoreLinq.Extensions;

namespace Moggle
{

/// <summary>
/// Creates a game by choosing from a pre-approved list
/// </summary>
public abstract record WhitelistGameMode : IMoggleGameMode
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public Solver CreateSolver(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var solveSettings = GetSolveSettings(settings);

        var solver = new Solver(wordList, solveSettings);

        return solver;
    }

    /// <inheritdoc />
    public MoggleBoard CreateBoard(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var seed = Seed.Get(settings);

        var random = RandomHelper.GetRandom(seed);
        var array  = GetLetters(random);

        var board = new MoggleBoard(array, Columns);
        return board;
    }

    public abstract MoggleBoard GenerateCuratedRandomBoard(Random random);

    public abstract ImmutableArray<Letter> GetLetters(Random random);
    public abstract int Columns { get; }
    public abstract SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings);

    /// <inheritdoc />
    public TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings) =>
        TimeSituation.Infinite.Instance;

    /// <inheritdoc />
    public Animation? GetAnimation(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        if (AnimateSetting.Get(settings))
        {
            var board  = CreateBoard(settings, wordList);
            var solver = CreateSolver(settings, wordList);

            var words = solver.GetPossibleSolutions(board);

            if (ReverseAnimationOrder)
                words = words.Reverse();

            return Animation.Create(words);

            //return Animation.CreateForAllSolutions(
            //    CreateBoard(settings, wordList),
            //    CreateSolver(settings, wordList),
            //    ReverseAnimationOrder
            //);
        }

        return null;
    }

    public virtual Setting.Bool AnimateSetting => new("Animate", false);

    public virtual Setting.String Seed =>
        new(nameof(Seed), null, "", "Random Seed") { GetRandomValue = GoodSeedHelper.GetGoodSeed };

    public virtual bool ReverseAnimationOrder => false;

    /// <inheritdoc />
    public abstract IEnumerable<Setting> Settings { get; }

    /// <inheritdoc />
    public abstract FoundWordsData GetFoundWordsData(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList);
}

/// <summary>
/// Creates a game by picking numbers out of a hat
/// </summary>
public abstract record BagGameMode : IMoggleGameMode
{
    public Rune GetRandomRune(Random random) => Letters.EnumerateRunes().Shuffle(random).First();

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public MoggleBoard CreateBoard(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var width  = Width.Get(settings);
        var height = Height.Get(settings);
        var seed   = Seed.Get(settings);

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
            array = GetLettersFromSeed(seed, size);
        }

        var board = new MoggleBoard(array, width);

        return board;
    }

    /// <inheritdoc />
    public Solver CreateSolver(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var solveSettings = GetSolveSettings(settings);

        var solver = new Solver(wordList, solveSettings);

        return solver;
    }

    public virtual ImmutableArray<Letter> GetLettersFromSeed(string seed, int size)
    {
        var allLetters = new List<Letter>();
        var random     = RandomHelper.GetRandom(seed);

        while (allLetters.Count < size)
        {
            allLetters.AddRange(
                Letters.EnumerateRunes()
                    .Shuffle(random)
                    .Take(size - allLetters.Count)
                    .Select(Letter.Create)
            );
        }

        var array = allLetters.Shuffle(random)
            .ToImmutableArray(); //shuffle again for to prevent patterns in very large grids

        return array;
    }

    /// <inheritdoc />
    public TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings)
    {
        if (AnimateSetting.Get(settings))
            return TimeSituation.Infinite.Instance;

        var ts = TimeSituation.Create(DurationSetting.Get(settings));
        return ts;
    }

    /// <inheritdoc />
    public Animation? GetAnimation(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        if (AnimateSetting.Get(settings))
        {

                var board = CreateBoard(settings, wordList);
                var solver = CreateSolver(settings, wordList);

                var words = solver.GetPossibleSolutions(board);

                words = words.Reverse();

                return Animation.Create(words);

            //    return Animation.CreateForAllSolutions(
            //    CreateBoard(settings, wordList),
            //    CreateSolver(settings, wordList),
            //    ReverseAnimationOrder
            //);
        }

        return null;
    }

    public virtual bool ReverseAnimationOrder => false;

    public abstract string Letters { get; }
    public abstract SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings);

    public virtual Setting.Integer Width => new(nameof(Width), 1, int.MaxValue, 4);
    public virtual Setting.Integer Height => new(nameof(Height), 1, int.MaxValue, 4);

    public virtual Setting.Integer DurationSetting => TimeSituation.Duration;

    public virtual Setting.Bool AnimateSetting => new("Animate", false);

    public virtual Setting.String Seed =>
        new(nameof(Seed), null, "", "Random Seed") { GetRandomValue = GoodSeedHelper.GetGoodSeed };

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
            yield return AnimateSetting;
        }
    }

    /// <inheritdoc />
    public FoundWordsData GetFoundWordsData(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList)
    {
        return new FoundWordsData.OpenSearchData(ImmutableDictionary<FoundWord, bool>.Empty);
    }
}

}
