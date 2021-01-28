using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Words;

namespace Moggle
{

public class Solver
{
    private Solver(IReadOnlySet<string> legalWords, IReadOnlySet<string> legalPrefixes)
    {
        _legalWords    = legalWords;
        _legalPrefixes = legalPrefixes;
    }

    public static async Task<Solver> InitializeAsync(CancellationToken ct)
    {
        var words = await new DictionaryHelper().GetNormalWordsAsync(ct);

        var legalWords = words.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var legalPrefixes = legalWords.SelectMany(GetAllPrefixes)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new Solver(legalWords, legalPrefixes);
    }

    public static Solver Initialize()
    {
        var words = new DictionaryHelper().GetNormalWords();

        var legalWords = words.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var legalPrefixes = legalWords.SelectMany(GetAllPrefixes)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new Solver(legalWords, legalPrefixes);
    }

    private readonly IReadOnlySet<string> _legalWords;
    private readonly IReadOnlySet<string> _legalPrefixes;

    private static IEnumerable<string> GetAllPrefixes(string s)
    {
        for (var length = 1; length < s.Length; length++)
            yield return s.Substring(0, length);
    }

    public IEnumerable<string> GetPossibleWords(MoggleBoard board)
    {
        var finder = new WordFinder(board, this);

        finder.Run();

        return finder.WordsSoFar;
    }

    private class WordFinder
    {
        public WordFinder(MoggleBoard board, Solver solver)
        {
            Board   = board;
            _solver = solver;
        }

        public readonly ISet<string> WordsSoFar =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private Solver _solver;

        public MoggleBoard Board { get; }

        public ConcurrentQueue<(string prefix, ImmutableList<Coordinate> usedCoordinates)> Queue =
            new();

        public void Run()
        {
            foreach (var coordinate in Board.GetAllCoordinates())
            {
                var prefix = Board.GetLetterAtCoordinate(coordinate).WordText;
                var list   = ImmutableList.Create(coordinate);

                Queue.Enqueue((prefix, list));
            }

            while (Queue.TryDequeue(out var a))
            {
                FindWords(a.prefix, a.usedCoordinates);
            }
        }

        private void FindWords(string prefix, ImmutableList<Coordinate> usedCoordinates)
        {
            var c = usedCoordinates.Last();

            foreach (var adjacentCoordinate in c.GetAdjacentCoordinates(Board.MaxCoordinate)
                .Except(usedCoordinates))
            {
                var l         = Board.GetLetterAtCoordinate(adjacentCoordinate);
                var newPrefix = prefix + l.WordText;

                if (_solver._legalWords.Contains(newPrefix))
                    WordsSoFar.Add(newPrefix);

                if (_solver._legalPrefixes.Contains(newPrefix))
                {
                    Queue.Enqueue((newPrefix, usedCoordinates.Add(adjacentCoordinate)));
                }
            }
        }
    }
}

}
