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
        LegalWords    = legalWords;
        LegalPrefixes = legalPrefixes;
    }

    public static Solver FromWordList(IEnumerable<string> words)
    {
        var legalWords = words.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var legalPrefixes = legalWords.SelectMany(GetAllPrefixes)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new Solver(legalWords, legalPrefixes);
    }

    public static async Task<Solver> FromDictionaryHelperAsync(CancellationToken ct)
    {
        var words = await new DictionaryHelper().GetNormalWordsAsync(ct);
        return FromWordList(words);
    }

    public static Solver FromDictionaryHelperAsync()
    {
        var words = new DictionaryHelper().GetNormalWords();
        return FromWordList(words);
    }

    public readonly IReadOnlySet<string> LegalWords;
    public readonly IReadOnlySet<string> LegalPrefixes;

    private static IEnumerable<string> GetAllPrefixes(string s)
    {
        for (var length = 1; length < s.Length; length++)
            yield return s.Substring(0, length);
    }

    public IEnumerable<string> GetPossibleWords(MoggleBoard board)
    {
        var finder = new WordFinder(board, this);

        finder.Run();

        return finder.WordsSoFar.OrderBy(x => x);
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

                if (_solver.LegalWords.Contains(newPrefix))
                    WordsSoFar.Add(newPrefix);

                if (_solver.LegalPrefixes.Contains(newPrefix))
                {
                    Queue.Enqueue((newPrefix, usedCoordinates.Add(adjacentCoordinate)));
                }
            }
        }
    }
}

}
