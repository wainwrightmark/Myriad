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

    [Fact]
    public void NumberFind()
    {
        var db  = GetDB();
        var ids = db.Table<CenturionGame>().Select(x => x.BoardId).ToHashSet();

        var existingIds = new ConcurrentDictionary<string, string>(
            ids.Select(x => new KeyValuePair<string, string>(x, x))
        );

        Stopwatch sw     = Stopwatch.StartNew();
        var       boards = NumberCreator.CreateBoards(NumbersGameMode.Instance, new Random(1), x=>x.Letters[1].ButtonText.Equals("!"));

        var solver = NumbersGameMode.Instance.CreateSolver(
            ImmutableDictionary<string, string>.Empty,
            WordList.LazyInstance
        );

        foreach (var board in boards)
        {
            if (existingIds.TryAdd(board.UniqueKey, board.UniqueKey))
            {
                var cg = CenturionGame.Create(board, solver, NumbersGameMode.Instance.Name);

                TestOutputHelper.WriteLine($"{sw.Elapsed}: {cg}");

                db.InsertOrReplace(cg);
            }
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

        Stopwatch sw     = Stopwatch.StartNew();
        var       boards = NumberCreator.CreateBoards(RomanGameMode.Instance, new Random(1), null);

        var solver = RomanGameMode.Instance.CreateSolver(
            ImmutableDictionary<string, string>.Empty,
            WordList.LazyInstance
        );

        foreach (var board in boards)
        {
            if (existingIds.TryAdd(board.UniqueKey, board.UniqueKey))
            {
                var cg = CenturionGame.Create(board, solver, RomanGameMode.Instance.Name);

                TestOutputHelper.WriteLine($"{sw.Elapsed}: {cg}");

                db.InsertOrReplace(cg);
            }
        }
    }
}

}
