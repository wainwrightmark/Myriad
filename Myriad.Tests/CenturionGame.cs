using System.Collections.Generic;
using System.Linq;
using SQLite;

namespace Myriad.Tests;

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
            BoardId             = board.UniqueKey,
            Width               = board.Columns,
            PossibleSolutions   = solutions.Count,
            MaxSolution         = solutions.Max(),
            MinSolution         = solutions.Min(),
            MaxContiguous       = maxContiguous,
            MinContiguous       = minContiguous,
            OneHundredSolutions = Enumerable.Range(1, 100).Count(solutions.Contains),
            Operators           = board.Letters.Count(x => OperatorCharacters.Contains(x.WordText)),
            Letters             = board.Letters.Count(x => x.WordText.All(char.IsLetter)),
            Numbers             = board.Letters.Count(x => x.WordText.All(char.IsDigit)),
            GameMode            = gameMode
        };

        return cg;
    }

    private static readonly HashSet<string> OperatorCharacters =
        (new[] { '+', '-', '*', '/', '=', '^' }).Select(x => x.ToString()).ToHashSet();
}