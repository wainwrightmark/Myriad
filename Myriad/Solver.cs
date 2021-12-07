﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Myriad.MathParser;

namespace Myriad;

public record Solver(WordList WordList, SolveSettings SolveSettings)
{
    public Solver(Lazy<WordList> wordList, SolveSettings solveSettings) : this(
        solveSettings.AllowWords ? wordList.Value : WordList.Empty,
        solveSettings
    ) { }

    public WordCheckResult CheckLegal(string s, ImmutableList<Coordinate> path)
    {
        if (SolveSettings.MinWordLength.HasValue && s.Length >= SolveSettings.MinWordLength
                                                 && WordList.LegalWords.Contains(s))
            return new WordCheckResult.Legal(new StringWord(s, path));

        if (SolveSettings.AllowMath && s.All(IsMath))
        {
            if (SolveSettings.AllowTrueEquations && Parser.IsValidEquation(s))
                return new WordCheckResult.Legal(new EquationWord(s, path));

            if (SolveSettings.MathExpressionsRange is not null)
            {
                var w = ExpressionWord.TryCreate(s, path);

                if (w is not null)
                {
                    if (SolveSettings.MathExpressionsRange.Value.Min <= w.Result
                     && w.Result <= SolveSettings.MathExpressionsRange.Value.Max)
                        return new WordCheckResult.Legal(w);

                    return new WordCheckResult.Illegal(w);
                }
            }
        }

        return WordCheckResult.Invalid.Instance;
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

            if (!char.IsLetterOrDigit(first) && first != '-')
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

        if (Parser.RomanNumerals.Contains(c))
            return true;

        return false;
    }

    private static readonly IReadOnlySet<char> Operators = new HashSet<char>()
    {
        '+',
        '=',
        '-',
        '*',
        '/',
        '^',
        '!'
    };

    private static bool MathCanFollow(char a, char b)
    {
        if (char.IsLetterOrDigit(a) || char.IsLetterOrDigit(b)) //letter or digit for roman numerals
            return true;

        //A and b are both operators

        if (b == '-' || b == '!')
            return true;

        return false;
    }

    public IEnumerable<FoundWord> GetPossibleSolutions(Board board)
    {
        int? maxWords = null;

        if (SolveSettings.MathExpressionsRange.HasValue && !SolveSettings.AllowWords
                                                        && !SolveSettings.AllowTrueEquations)
        {
            maxWords = SolveSettings.MathExpressionsRange.Value.Max
                     - SolveSettings.MathExpressionsRange.Value.Min;
        }

        var finder = new WordFinder(board, this, maxWords);

        finder.Run();

        return finder.WordsSoFar.Keys.OrderBy(x => x);
    }

    private class WordFinder
    {
        public WordFinder(Board board, Solver solver, int? maxWords)
        {
            Board    = board;
            _solver  = solver;
            MaxWords = maxWords;
        }

        public readonly ConcurrentDictionary<FoundWord, byte> WordsSoFar =
            new();

        private readonly Solver _solver;

        private Board Board { get; }

        private int? MaxWords {get;}

        private readonly ConcurrentQueue<(string prefix, ImmutableList<Coordinate> usedCoordinates)>
            _queue =
                new();

        public void Run()
        {
            foreach (var coordinate in Board.GetAllCoordinates())
            {
                var prefix = Board.GetLetterAtCoordinate(coordinate).WordText;

                var list = ImmutableList.Create(coordinate);
                var w    = _solver.CheckLegal(prefix, list);

                if (w is WordCheckResult.Legal legalWord)
                    WordsSoFar.TryAdd(legalWord.Word, 0);

                _queue.Enqueue((prefix, list));
            }

            while (_queue.TryDequeue(out var a) && (MaxWords is null || WordsSoFar.Count <= MaxWords))
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
                var newList   = usedCoordinates.Add(adjacentCoordinate);

                var w = _solver.CheckLegal(newPrefix, newList);

                if (w is WordCheckResult.Legal legalWord)
                    WordsSoFar.TryAdd(legalWord.Word, 0);

                if (_solver.IsLegalPrefix(newPrefix))
                {
                    _queue.Enqueue((newPrefix, newList));
                }
            }
        }
    }
}