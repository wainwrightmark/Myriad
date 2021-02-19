using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record MoggleState(
    MoggleBoard Board,
    Solver Solver,
    DateTime? FinishTime,
    int Rotation,
    ImmutableList<Coordinate> ChosenPositions,
    ImmutableSortedSet<string> FoundWords,
    ImmutableHashSet<string> DisabledWords,
    ImmutableList<string>? CheatWords)
{
    public static readonly MoggleState DefaultState =
        StartNewGame(
            WordList.FromWords(new[] { "welcome", "moggle", "come" }),
            ModernGameMode.Instance, ImmutableDictionary<string, string>.Empty,
            0
        );

    public static MoggleState StartNewGame(
        WordList wordList,
        IMoggleGameMode gameMode,
        ImmutableDictionary<string, string> settings,
        int duration)
    {
        var (board, solveSettings) = gameMode.CreateGame(settings);

        MoggleState newState = new(
            board,
            new Solver(wordList, solveSettings),
            DateTime.Now.AddSeconds(duration),
            0,
            ImmutableList<Coordinate>.Empty,
            ImmutableSortedSet<string>.Empty,
            ImmutableHashSet<string>.Empty,
            null
        );

        return newState;
    }

    public int MaxDimension => Math.Max(Board.Width, Board.Height);

    public bool IsMoveLegal(Coordinate coordinate) => TryGetMoveResult(coordinate) != null;

    public MoggleState? TryGetMoveResult(Coordinate coordinate)
    {
        if (FinishTime == null)
            return null;

        if (!ChosenPositions.Any())
            return this with { ChosenPositions = ChosenPositions.Add(coordinate) };

        if (ChosenPositions.Last().Equals(coordinate))
        {
            if (ChosenPositions.Count >= Solver.SolveSettings.MinimumTermLength) //Complete a word
            {
                var word = string.Join(
                    "",
                    ChosenPositions.Select(GetLetterAtCoordinate).Select(x => x.WordText)
                );

                if (!Solver.IsLegal(word))
                    return null;

                return this with
                {
                    ChosenPositions = ImmutableList<Coordinate>.Empty,
                    FoundWords = FoundWords.Add(word)
                };
            }

            if (ChosenPositions.Count <= Solver.SolveSettings.MinimumTermLength) //Give up on this path
            {
                return this with { ChosenPositions = ImmutableList<Coordinate>.Empty };
            }

            return null;
        }

        var index = ChosenPositions.LastIndexOf(coordinate);

        switch (index)
        {
            //Give up on this path
            case 0 or 1: return this with { ChosenPositions = ImmutableList<Coordinate>.Empty };
            //Go back some number of steps
            case > 1:
                return this with
                {
                    ChosenPositions = ChosenPositions.Take(index + 1).ToImmutableList()
                };
        }

        if (ChosenPositions.Last().IsAdjacent(coordinate)) //add this coordinate to the list
            return this with { ChosenPositions = ChosenPositions.Add(coordinate) };

        return null;
    }

    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var newCoordinate = coordinate.Rotate(Board.MaxCoordinate, Rotation);
        return Board.GetLetterAtCoordinate(newCoordinate);
    }

    public static int ScoreWord(int length)
    {
        return length switch
        {
            < 3  => 0,
            3    => 1,
            4    => 1,
            5    => 2,
            6    => 3,
            7    => 4,
            >= 8 => 11
        };
    }

    public int Score
    {
        get
        {
            return Words.Sum(s => ScoreWord(s.Length));
        }
    }

    public int NumberOfWords => Words.Count();

    private IEnumerable<string> Words
    {
        get
        {
            if (CheatWords != null)
                return CheatWords;

            return FoundWords.Except(DisabledWords);
        }
    }
}

}
