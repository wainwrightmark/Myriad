using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Myriad.Creator;
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

    private static SQLiteConnection GetDB()
    {
        var databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Centurion.db"
        );

        var db = new SQLiteConnection(databasePath);
        db.CreateTable<CenturionGame>();

        return db;
    }

    private static void CreateGames(
        int startIndex,
        int numberToCreate,
        ITestOutputHelper testOutputHelper)
    {
        var db  = GetDB();
        var ids = db.Table<CenturionGame>().Select(x => x.BoardId).ToHashSet();

        var existingIds = new ConcurrentDictionary<string, string>(
            ids.Select(x => new KeyValuePair<string, string>(x, x))
        );

        var source = Enumerable.Range(startIndex, numberToCreate).AsParallel();

        source.ForAll(CalculateGame);

        void CalculateGame(int i)
        {
            NumberGameMode numberGameMode =
                i % 2 == 0 ? NumbersGameMode.Instance : RomanGameMode.Instance;

            var board = numberGameMode.GenerateCuratedRandomBoard(new Random(i));

            var solver = new Solver(
                WordList.LazyInstance,
                numberGameMode.GetSolveSettings(ImmutableDictionary<string, string>.Empty)
            );

            if (existingIds.TryAdd(board.UniqueKey, board.UniqueKey))
            {
                var cg = CenturionGame.Create(board, solver, numberGameMode.Name);

                if (i - startIndex < 100 || i % 1000 == 0 || cg.PossibleSolutions >= 100)
                    testOutputHelper.WriteLine($"{i}: {cg}");

                db.InsertOrReplace(cg);
            }
        }
    }

    [Theory]
    [InlineData(0,     100)]
    [InlineData(18000, 1000000)]
    public void CreateNumbers(int startIndex, int numberToCreate)
    {
        CreateGames(
            startIndex,
            numberToCreate,
            TestOutputHelper
        );
    }

    private static bool HasCharacterInCorner(Board board, Letter l) => board.Letters[0] == l
     || board.Letters[2] == l || board.Letters[6] == l || board.Letters[8] == l;

    private static bool HasCharacterInCross(Board board, Letter l) => board.Letters[1] == l
     || board.Letters[3] == l || board.Letters[5] == l || board.Letters[7] == l;

    private static bool HasCharacterInCentre(Board board, Letter l) => board.Letters[4] == l;

    [Fact]
    public void WordFind()
    {
        var seed = 3;

        Stopwatch sw = Stopwatch.StartNew();

        var solver = WordsGameMode.Instance.CreateSolver(
            ImmutableDictionary<string, string>.Empty,
            WordList.LazyInstance
        );

        var boards = NumberCreator.CreateBoards(
            WordsGameMode.Instance,
            solver,
            new Random(seed),
            4,
            400,
            null
        );

        foreach (var board in boards)
        {
            var solutions = solver.GetPossibleSolutions(board).Count();
            TestOutputHelper.WriteLine($"{sw.Elapsed}: {board.UniqueKey}: {solutions} Solutions");
        }
    }


    [Fact]
    public void NumberFind()
    {
        var seed = 3;
        var db   = GetDB();
        var ids  = db.Table<CenturionGame>().Select(x => x.BoardId).ToHashSet();

        var existingIds = new ConcurrentDictionary<string, string>(
            ids.Select(x => new KeyValuePair<string, string>(x, x))
        );

        Stopwatch sw = Stopwatch.StartNew();

        var solver = NumbersGameMode.Instance.CreateSolver(
            ImmutableDictionary<string, string>.Empty,
            WordList.LazyInstance
        );

        var letter = Letter.Create('_');

        var boards = NumberCreator.CreateBoards(
            NumbersGameMode.Instance,
            solver,
            new Random(seed),
            3,
            100,
            x => HasCharacterInCentre(x, letter)
        );

        foreach (var board in boards)
        {
            var exists = existingIds.TryAdd(board.UniqueKey, board.UniqueKey);
            var cg     = CenturionGame.Create(board, solver, NumbersGameMode.Instance.Name);
            TestOutputHelper.WriteLine($"{sw.Elapsed}:  {cg}" + (exists ? " - new" : ""));

            if (exists)
                db.InsertOrReplace(cg);
        }
    }

    [Fact]
    public void RomanFind()
    {
        var db  = GetDB();
        var ids = db.Table<CenturionGame>().Select(x => x.BoardId).ToHashSet();

        var existingIds = new ConcurrentDictionary<string, string>(
            ids.Select(x => new KeyValuePair<string, string>(x, x))
        );

        Stopwatch sw = Stopwatch.StartNew();

        var solver = RomanGameMode.Instance.CreateSolver(
            ImmutableDictionary<string, string>.Empty,
            WordList.LazyInstance
        );

        var boards = NumberCreator.CreateBoards(
            RomanGameMode.Instance,
            solver,
            new Random(3),
            3,
            100,
            null
        );

        foreach (var board in boards)
        {
            var exists = existingIds.TryAdd(board.UniqueKey, board.UniqueKey);
            var cg     = CenturionGame.Create(board, solver, RomanGameMode.Instance.Name);
            TestOutputHelper.WriteLine($"{sw.Elapsed}:  {cg}" + (exists ? " - new" : ""));

            if (exists)
                db.InsertOrReplace(cg);
        }
    }
}

}
