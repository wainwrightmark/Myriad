using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using SQLite;
using Xunit;
using Xunit.Abstractions;

namespace Myriad.Tests
{

public class GameFinder
{
    public ITestOutputHelper TestOutputHelper { get; }

    public GameFinder(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    private static void CreateGames(
        int startIndex,
        int numberToCreate,
        ITestOutputHelper testOutputHelper,
        WhitelistGameMode wgm)
    {
        var databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Centurion.db"
        );

        var db = new SQLiteConnection(databasePath);
        db.CreateTable<CenturionGame>();

        var ids = db.Table<CenturionGame>().Select(x => x.BoardId).ToHashSet();

        var existingIds = new ConcurrentDictionary<string, string>(
            ids.Select(x => new KeyValuePair<string, string>(x, x))
        );

        var source = Enumerable.Range(startIndex, numberToCreate).AsParallel();

        source.ForAll(CalculateGame);

        void CalculateGame(int i)
        {
            var board = wgm.GenerateCuratedRandomBoard(new Random(i));

            var solver = new Solver(
                WordList.LazyInstance,
                wgm.GetSolveSettings(ImmutableDictionary<string, string>.Empty)
            );

            if (existingIds.TryAdd(board.UniqueKey, board.UniqueKey))
            {
                var cg = CenturionGame.Create(board, solver, wgm.Name);

                if(i < 100 || i % 100 == 0 || cg.PossibleSolutions >= 100)
                    testOutputHelper.WriteLine($"{i}: {cg}");

                db.InsertOrReplace(cg);
            }
        }
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(0, 1000000)]
    public void CreateCentury(int startIndex, int numberToCreate)
    {
        CreateGames(
            startIndex,
            numberToCreate,
            TestOutputHelper, CenturyGameMode.Instance
        );
    }

    [Theory]
    [InlineData(0, 100)]
    //[InlineData(0, 1000000)]
    public void CreateRoman(int startIndex, int numberToCreate)
    {
        CreateGames(
            startIndex,
            numberToCreate,
            TestOutputHelper,
            RomanGameMode.Instance
        );
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
            MaxContiguous,
            OneHundredSolutions,
            Operators,
            Numbers,
            Letters,
            GameMode
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
    public int Numbers { get; set; }
    public int Letters { get; set; }

    public string GameMode { get; set; }

    public static CenturionGame Create(Board board, Solver solver, string gameMode)
    {
        var solutions = solver.GetPossibleSolutions(board)
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
            BoardId = board.UniqueKey,
            Width = board.Columns,
            PossibleSolutions = solutions.Count,
            MaxSolution = solutions.Max(),
            MinSolution = solutions.Min(),
            MaxContiguous = maxContiguous,
            MinContiguous = minContiguous,
            OneHundredSolutions = Enumerable.Range(1, 100).Count(solutions.Contains),
            Operators = board.Letters.Count(x => OperatorCharacters.Contains(x.WordText)),
            Letters = board.Letters.Count(x => x.WordText.All(char.IsLetter)),
            Numbers = board.Letters.Count(x => x.WordText.All(char.IsDigit)),
            GameMode = gameMode
        };

        return cg;
    }

    private static readonly HashSet<string> OperatorCharacters =
        (new[] { '+', '-', '*', '/', '=', '^' }).Select(x => x.ToString()).ToHashSet();
}

}
