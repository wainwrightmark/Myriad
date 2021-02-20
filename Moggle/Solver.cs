﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moggle.MathParser;

namespace Moggle
{

public record Solver(WordList WordList, SolveSettings SolveSettings)
{
    public FoundWord? CheckLegal(string s)
    {
        if (SolveSettings.MinWordLength.HasValue && s.Length >= SolveSettings.MinWordLength
                                                 && WordList.LegalWords.Contains(s))
            return new StringWord(s);

        if (SolveSettings.AllowMath && s.All(IsMath))
        {
            if (SolveSettings.AllowTrueEquations && Parser.IsValidEquation(s))
                return new EquationWord(s);

            if (SolveSettings.MathExpressionsRange is not null)
            {
                var w = MathExpressionWord.TryCreate(s);

                if (w is not null
                 && SolveSettings.MathExpressionsRange.Value.Min <= w.Result
                 && w.Result <= SolveSettings.MathExpressionsRange.Value.Max)
                    return w;
            }
        }

        return null;
    }

    public bool IsLegalPrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return true;

        if (SolveSettings.AllowWords && WordList.LegalPrefixes.Contains(prefix))
            return true;

        if (SolveSettings.AllowMath && prefix.All(IsMath))
        {
            var first = prefix[0];

            if (!char.IsNumber(first) && first != '-')
                return false;

            for (var i = 1;
                 i < prefix.Length;
                 i++) //Check that two operators never follow one another
            {
                var a = prefix[i - 1];
                var b = prefix[i];

                if (!MathCanFollow(a, b))
                    return false;
            }

            return true;
        }

        return false;
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
        '+',
        '=',
        '-',
        '*',
        '/'
    };

    private static bool MathCanFollow(char a, char b)
    {
        if (char.IsDigit(a) || char.IsDigit(b))
            return true;

        //A and b are both operators
        if (b == '-')
            return true;

        return false;
    }

    public IEnumerable<FoundWord> GetPossibleSolutions(MoggleBoard board)
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

        public readonly ISet<FoundWord> WordsSoFar = new HashSet<FoundWord>();

        private readonly Solver _solver;

        private MoggleBoard Board { get; }

        private readonly ConcurrentQueue<(string prefix, ImmutableList<Coordinate> usedCoordinates)>
            _queue =
                new();

        public void Run()
        {
            foreach (var coordinate in Board.GetAllCoordinates())
            {
                var prefix = Board.GetLetterAtCoordinate(coordinate).WordText;

                var w = _solver.CheckLegal(prefix);

                if (w != null)
                    WordsSoFar.Add(w);

                var list = ImmutableList.Create(coordinate);

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

                var w = _solver.CheckLegal(newPrefix);

                if (w != null)
                    WordsSoFar.Add(w);

                if (_solver.IsLegalPrefix(newPrefix))
                {
                    _queue.Enqueue((newPrefix, usedCoordinates.Add(adjacentCoordinate)));
                }
            }
        }
    }
}

}
