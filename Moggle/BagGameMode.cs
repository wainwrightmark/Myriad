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
    public (MoggleBoard board, Solver Solver)
        CreateGame(
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

            array = allLetters.Shuffle(random)
                .ToImmutableArray(); //shuffle again for to prevent patterns in very large grids
        }

        var board = new MoggleBoard(array, width);

        var solveSettings = GetSolveSettings(settings);

        var solver = new Solver(wordList, solveSettings);

        return (board, solver);
    }

    /// <inheritdoc />
    public TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings)
    {
        if(AnimateSetting.Get(settings))
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
            var (board, solver) = CreateGame(settings, wordList);

            var paths = solver.GetPossiblePaths(board).Select(x => x.Value);

            if (ReverseAnimationOrder)
                paths = paths.Reverse();

            var steps = new List<Step>();

            var previous = ImmutableList<Coordinate>.Empty;

            foreach (var path in paths)
            {
                var stepsInCommon =
                    path.Zip(previous)
                        .TakeWhile(x => x.First.Equals(x.Second))
                        .Select(x => x.First)
                        .ToList();

                if (!stepsInCommon.Any())
                {
                    if (previous.Any())
                        steps.Add(new Step.Move(previous.Last())); //abandon

                    steps.AddRange(path.Select(x => new Step.Move(x)));
                }
                else if (stepsInCommon.Count == previous.Count) //continue
                {
                    steps.AddRange(path.Skip(stepsInCommon.Count).Select(x => new Step.Move(x)));
                }
                else //backtrack then continue
                {
                    steps.Add(new Step.Move(stepsInCommon.Last()));
                    steps.AddRange(path.Skip(stepsInCommon.Count).Select(x => new Step.Move(x)));
                }

                previous = path;
            }

            if (!steps.Any())
                return null;

            steps.Add(steps.Last());
            steps.Add(new Step.Rotate(1));

            return new Animation(steps.ToImmutableList());
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
}

}
