using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;

namespace Myriad.Creator
{

public static class NumberCreator
{
    private record BoardWithSolutions(Board Board, int Solutions) : IComparable
    {
        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            if (obj is BoardWithSolutions bws)
            {
                return Solutions.CompareTo(bws.Solutions);
            }

            return 0;
        }
    }

    public static IEnumerable<Board> CreateBoards(
        NumberGameMode gameMode,
        Solver solver,
        Random random,
        Func<Board, bool>? condition)
    {

        var board1 = new Board(Enumerable.Repeat(Letter.Create('_'), 9).ToImmutableArray(), 3);

        var legalLetters = gameMode.LegalLetters.ToImmutableList();
        var boards       = new HashSet<string>() { board1.UniqueKey };

        var queue = new ConcurrentPriorityQueue<BoardWithSolutions>() { new(board1, 0) };

        while (queue.TryTake(out var b))
        {
            foreach (var solution in GetAllMutations(b))
            {
                queue.Add(solution);

                if (solution.Solutions >= 100)
                    yield return solution.Board;
            }
        }

        IEnumerable<BoardWithSolutions> GetAllMutations(BoardWithSolutions bws)
        {
            var indexes = Enumerable.Range(0, bws.Board.Letters.Length).Shuffle(random);

            var pairs = indexes.SelectMany(
                index => legalLetters.Shuffle(random).Select(newLetter => (newLetter, index))
            );

            var betterSolutions = pairs
                .SelectMany(p => GetBetterSolutions(p.newLetter, p.index))
                .AsParallel();

            foreach (var solution in betterSolutions)
            {
                yield return solution;
            }

            IEnumerable<BoardWithSolutions> GetBetterSolutions(
                Letter newLetter,
                int index)
            {
                var letter = bws.Board.Letters[index];

                if (newLetter.Equals(letter))
                    yield break;

                var newLetters = bws.Board.Letters.SetItem(index, newLetter);

                var newBoard = new Board(newLetters, bws.Board.Columns);

                if (condition is not null && !condition(newBoard))
                    yield break;

                if (boards.Add(newBoard.UniqueKey))
                {
                    var solutions = solver.GetPossibleSolutions(newBoard).Count();

                    if (solutions >= bws.Solutions)
                        yield return new(newBoard, solutions);
                }
            }
        }
    }
}

}
