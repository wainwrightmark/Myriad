using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using Moggle.States;
using SQLite;
using Xunit;
using Xunit.Abstractions;

namespace Moggle.Tests
{

public class CenturionFinder
{
    public ITestOutputHelper TestOutputHelper { get; }

    public CenturionFinder(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(0, 200000)]
    //[InlineData(0, 1000000)]
    public void Create(int startIndex, int numberToCreate)
    {
        var databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Centurion.db"
        );

        var db = new SQLiteConnection(databasePath);
        db.CreateTable<CenturionGame>();

        var existingIds = db.Table<CenturionGame>().Select(x => x.BoardId).ToHashSet();

        var source = Enumerable.Range(startIndex, numberToCreate).AsParallel();

        source.ForAll(CalculateGame);

        void CalculateGame(int i)
        {
            var dict = new Dictionary<string, string>()
            {
                { CenturyGameMode.Instance.Seed.Name, i.ToString() },
                { CenturyGameMode.Minimum.Name, int.MinValue.ToString() },
                { CenturyGameMode.Maximum.Name, int.MaxValue.ToString() },
            };

            var game =
                MoggleState.StartNewGame(
                    WordList.Empty,
                    CenturyGameMode.Instance,
                    dict.ToImmutableDictionary(x => x.Key, x => x.Value)
                );

            if (existingIds.Add(game.Board.UniqueKey))
            {
                var cg = CenturionGame.Create(game);

                TestOutputHelper.WriteLine(cg.ToString());

                db.InsertOrReplace(cg);
            }
        }
    }
}

public class CenturionGame
{
    /// <inheritdoc />
    public override string ToString()
    {
        return new
        {
            BoardId,
            PossibleSolutions,
            MinSolution,
            MaxSolution,
            MinContiguous,
            MaxContiguous
        }.ToString()!;
    }

    [PrimaryKey] public string BoardId { get; set; }

    public int Width { get; set; }
    [Indexed] public int PossibleSolutions { get; set; }
    public int MaxSolution { get; set; }
    public int MinSolution { get; set; }
    public int MaxContiguous { get; set; }
    public int MinContiguous { get; set; }

    public int OneHundredSolutions { get; set; }

    public int Operators { get; set; }

    public static CenturionGame Create(MoggleState state)
    {
        var solutions = state.Solver.GetPossibleSolutions(state.Board)
            .OfType<ExpressionWord>()
            .Select(x => x.Result)
            .DefaultIfEmpty(0)
            .ToHashSet();

        int minContiguous;
        int maxContiguous;

        var lowestInt = solutions.Where(x => x > 0).DefaultIfEmpty(0).Min();

        if (lowestInt <= 0)
        {
            minContiguous = 0;
            maxContiguous = 0;
        }
        else
        {
            minContiguous = lowestInt;

            while (solutions.Contains(minContiguous - 1))
                minContiguous--;

            maxContiguous = lowestInt;

            while (solutions.Contains(maxContiguous + 1))
                maxContiguous++;
        }

        var cg = new CenturionGame()
        {
            BoardId             = state.Board.UniqueKey,
            Width               = state.Board.Columns,
            PossibleSolutions   = solutions.Count,
            MaxSolution         = solutions.Max(),
            MinSolution         = solutions.Min(),
            MaxContiguous       = maxContiguous,
            MinContiguous       = minContiguous,
            OneHundredSolutions = Enumerable.Range(1, 100).Count(solutions.Contains),
            Operators           = state.Board.Letters.Count(x => !int.TryParse(x.WordText, out _))
        };

        return cg;
    }
}

}
