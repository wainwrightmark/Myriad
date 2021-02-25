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
            var (board, solver) = CreateGame(settings, wordList);


            var paths =
                RemoveRedundant(
                    solver.GetPossiblePaths(board),
                    solver,
                    board
                );

            var sortedPaths =
                ReverseAnimationOrder?
                paths.OrderByDescending(x=>x, ListComparer<Coordinate>.Instance) :
                paths.OrderBy(x=>x, ListComparer<Coordinate>.Instance);




            var steps = new List<Step>();

            var previous = ImmutableList<Coordinate>.Empty;

            foreach (var path in sortedPaths)
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

    private static IEnumerable<ImmutableList<Coordinate>> RemoveRedundant(
        IEnumerable<KeyValuePair<FoundWord, ImmutableList<Coordinate>>> initial,
        Solver solver,
        MoggleBoard board)
    {
        var everything = initial.Select(
                x =>
                    (finalValue: x.Key, coordinates: x.Value,
                     subvalues: GetAllSubValues(x.Value).ToList())
            )
            .ToDictionary(x => x.finalValue);

        var cont = true;

        while (cont)
        {
            cont = false;

            var singleSourceWords = everything.Values
                .SelectMany(x =>  Enumerable.Append(x.subvalues,x.finalValue))
                .GroupBy(x => x)
                .Where(x => x.Count() == 1)
                .Select(x => x.Key)
                .ToList();

            foreach (var singleSourceWord in singleSourceWords)
            {
                if (everything.Remove(singleSourceWord, out var av))
                {
                    cont = true; //we have successfully removed something
                    yield return av.coordinates;
                    foreach (var subWord in av.subvalues)
                        everything.Remove(subWord);
                }
            }
        }

        while (everything.Any()) //Hopefully there will not be much left
        {
            var e = everything.First();

            yield return e.Value.coordinates;

            everything.Remove(e.Key);

            foreach (var valueSubvalue in e.Value.subvalues)
            {
                everything.Remove(valueSubvalue);
            }
        }

        IEnumerable<FoundWord> GetAllSubValues(ImmutableList<Coordinate> list)
        {
            for (var i = 1; i < list.Count; i++)
            {
                var text = string.Join(
                    "",
                    list.Take(i).Select(board.GetLetterAtCoordinate).Select(x => x.WordText)
                );

                var r = solver.CheckLegal(text);

                if (r is WordCheckResult.Legal legal)
                    yield return legal.Word;
            }
        }
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
