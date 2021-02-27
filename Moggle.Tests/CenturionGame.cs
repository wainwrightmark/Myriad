using System;
using System.Collections.Concurrent;
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

public class GameFinder
{
    public ITestOutputHelper TestOutputHelper { get; }

    public GameFinder(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    private static void CreateGames(
        string gameMode,
        int startIndex,
        int numberToCreate,
        ITestOutputHelper testOutputHelper,
        Func<string, (IMoggleGameMode, IReadOnlyDictionary<string, string>)> createGame)
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
            var (moggleGameMode, readOnlyDictionary) = createGame(i.ToString());

            var game =
                MoggleState.StartNewGame(
                    WordList.Empty,
                    moggleGameMode,
                    readOnlyDictionary.ToImmutableDictionary(x => x.Key, x => x.Value)
                );

            if (existingIds.TryAdd(game.Board.UniqueKey, game.Board.UniqueKey))
            {
                var cg = CenturionGame.Create(game, gameMode);

                testOutputHelper.WriteLine(cg.ToString());

                db.InsertOrReplace(cg);
            }
        }
    }

    [Theory]
    [InlineData(0, 200000)]
    //[InlineData(0, 1000000)]
    public void CreateCentury(int startIndex, int numberToCreate)
    {
        CreateGames(
            CenturyGameMode.Instance.Name,
            startIndex,
            numberToCreate,
            TestOutputHelper,
            s => (CenturyGameMode.Instance,
                  new Dictionary<string, string>()
                  {
                      { CenturyGameMode.Instance.Seed.Name, s },
                      { CenturyGameMode.Minimum.Name, 0.ToString() },
                      { CenturyGameMode.Maximum.Name, int.MaxValue.ToString() },
                  }
                )
        );
    }

    [Theory]
    [InlineData(0, 10000)]
    //[InlineData(0, 1000000)]
    public void CreateRoman(int startIndex, int numberToCreate)
    {
        CreateGames(
            RomanGameMode.Instance.Name,
            startIndex,
            numberToCreate,
            TestOutputHelper,
            s => (RomanGameMode.Instance,
                  new Dictionary<string, string>()
                  {
                      { RomanGameMode.Instance.Seed.Name, s },
                      { RomanGameMode.Minimum.Name, 0.ToString() },
                      { RomanGameMode.Maximum.Name, int.MaxValue.ToString() },
                  }
                )
        );
    }

    [Theory]
    [InlineData("VII-*+IIX")]
    public void CreateRomanWithString(string gameString)
    {
        CreateGames(
            RomanGameMode.Instance.Name,
            0,
            1,
            TestOutputHelper,
            _ => (FixedGameMode.Instance,
                  new Dictionary<string, string>()
                  {
                      { FixedGameMode.Letters.Name, gameString },
                      { FixedGameMode.Minimum.Name, 0.ToString() },
                      { FixedGameMode.Maximum.Name, int.MaxValue.ToString() },
                      { FixedGameMode.MinWordLength.Name, (-1).ToString() }
                  }
                )
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

    public static CenturionGame Create(MoggleState state, string gameMode)
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
            BoardId = state.Board.UniqueKey,
            Width = state.Board.Columns,
            PossibleSolutions = solutions.Count,
            MaxSolution = solutions.Max(),
            MinSolution = solutions.Min(),
            MaxContiguous = maxContiguous,
            MinContiguous = minContiguous,
            OneHundredSolutions = Enumerable.Range(1, 100).Count(solutions.Contains),
            Operators = state.Board.Letters.Count(x => OperatorCharacters.Contains(x.WordText)),
            Letters = state.Board.Letters.Count(x => x.WordText.All(char.IsLetter)),
            Numbers = state.Board.Letters.Count(x => x.WordText.All(char.IsDigit)),
            GameMode = gameMode
        };

        return cg;
    }

    private static readonly HashSet<string> OperatorCharacters =
        (new[] { '+', '-', '*', '/', '=', '^' }).Select(x => x.ToString()).ToHashSet();
}

}
