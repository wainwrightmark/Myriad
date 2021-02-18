using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Moggle
{

public class Solver
{
    private Solver(IReadOnlySet<string> legalWords, IReadOnlySet<string> legalPrefixes)
    {
        LegalWords    = legalWords;
        LegalPrefixes = legalPrefixes;
    }

    public bool IsLegal(string s)
    {
        if (LegalWords.Contains(s))
            return true;

        if (s.All(IsMath))
            return IsTrueMathOperation(s);

        return false;
    }

    private static bool IsTrueMathOperation(string s)
    {
        return MathParser.IsValidEquation(s);
    }

    private static bool IsMath(char c)
    {
        if (char.IsDigit(c))
            return true;

        if (Operators.Contains(c))
            return true;

        return false;
    }

    private static readonly IReadOnlySet<char> Operators = new HashSet<char>()
    {
        '+', '=', '-', '*','/'
    };

    public static Solver FromWordList(IEnumerable<string> words)
    {
        var sw = Stopwatch.StartNew();
        Console.WriteLine("Loading words from word list");
        var legalWords = words.ToHashSet(StringComparer.OrdinalIgnoreCase);
        Console.WriteLine($"{legalWords.Count} words found {sw.ElapsedMilliseconds}ms");

        var legalPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var legalWord in legalWords)
            AddAllPrefixes(legalWord, legalPrefixes);

        Console.WriteLine($"{legalPrefixes.Count} prefixes found {sw.ElapsedMilliseconds}ms");

        return new Solver(legalWords, legalPrefixes);
    }

    public static Solver FromResourceFile()
    {
        var words = Words.WordList.Split('\n');

        return FromWordList(words);
    }

    public readonly IReadOnlySet<string> LegalWords;
    public readonly IReadOnlySet<string> LegalPrefixes;

    private static void AddAllPrefixes(string s, ISet<string> set)
    {
        for (var length = s.Length - 1; length >= 1; length--)
        {
            var substring = s.Substring(0, length);

            if (!set.Add(substring))
                return; //We already have this prefix and therefore all prior prefixes
        }
    }

    public IEnumerable<string> GetPossibleWords(MoggleBoard board)
    {
        var finder = new WordFinder(board, this);

        finder.Run();

        return finder.WordsSoFar.OrderBy(x => x).Where(x=>x.Length >= board.MinWordLength);
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

        private readonly Solver _solver;

        private MoggleBoard Board { get; }

        private readonly ConcurrentQueue<(string prefix, ImmutableList<Coordinate> usedCoordinates)> _queue =
            new();

        public void Run()
        {
            foreach (var coordinate in Board.GetAllCoordinates())
            {
                var prefix = Board.GetLetterAtCoordinate(coordinate).WordText;
                var list   = ImmutableList.Create(coordinate);

                _queue.Enqueue((prefix, list));
            }

            while (_queue.TryDequeue(out var a))
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
                    _queue.Enqueue((newPrefix, usedCoordinates.Add(adjacentCoordinate)));
                }
            }
        }
    }
}

}
