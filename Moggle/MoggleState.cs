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
    ImmutableSortedSet<FoundWord> FoundWords,
    ImmutableHashSet<FoundWord> DisabledWords,
    ImmutableList<FoundWord>? CheatWords)
{
    public static readonly MoggleState DefaultState =
        StartNewGame(
            WordList.LazyInstance.Value,
            ModernGameMode.Instance,
            ImmutableDictionary<string, string>.Empty,
            120
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
            ImmutableSortedSet<FoundWord>.Empty,
            ImmutableHashSet<FoundWord>.Empty,
            null
        );

        return newState;
    }

    public int MaxDimension => Math.Max(Board.Width, Board.Height);

    public MoveResult TryGetMoveResult(Coordinate coordinate)
    {
        if (FinishTime == null)
            return MoveResult.IllegalMove.Instance;

        if (FinishTime.Value.CompareTo(DateTime.Now) < 0)
            return new MoveResult.TimeElapsed(this with { FinishTime = null });

        if (!ChosenPositions.Any())
            return new MoveResult.WordContinued(
                this with { ChosenPositions = ChosenPositions.Add(coordinate) }
            );

        if (ChosenPositions.Last().Equals(coordinate))
        {
            if (ChosenPositions.Count >= Solver.SolveSettings.MinimumTermLength) //Complete a word
            {
                var word = string.Join(
                    "",
                    ChosenPositions.Select(GetLetterAtCoordinate).Select(x => x.WordText)
                );

                var foundWord = Solver.CheckLegal(word);

                if (foundWord is null)
                    return MoveResult.InvalidWord.Instance;

                var stateWithWord = this with
                {
                    ChosenPositions = ImmutableList<Coordinate>.Empty,
                    FoundWords = FoundWords.Add(foundWord)
                };

                return new MoveResult.WordComplete(stateWithWord);
            }

            if (ChosenPositions.Count <= Solver.SolveSettings.MinimumTermLength
            ) //Give up on this path
            {
                return new MoveResult.WordAbandoned(
                    this with { ChosenPositions = ImmutableList<Coordinate>.Empty }
                );
            }

            return MoveResult.IllegalMove.Instance;
        }

        var index = ChosenPositions.LastIndexOf(coordinate);

        switch (index)
        {
            //Give up on this path
            case 0:
                return new MoveResult.WordAbandoned(
                    this with { ChosenPositions = ImmutableList<Coordinate>.Empty }
                );
            //Go back some number of steps
            case >= 1:
                return new MoveResult.MoveRetraced(
                    this with
                    {
                        ChosenPositions = ChosenPositions.Take(index + 1).ToImmutableList()
                    }
                );
        }

        if (ChosenPositions.Last().IsAdjacent(coordinate)) //add this coordinate to the list
            return new MoveResult.WordContinued(
                this with { ChosenPositions = ChosenPositions.Add(coordinate) }
            );

        return MoveResult.IllegalMove.Instance;
    }

    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var newCoordinate = coordinate.Rotate(Board.MaxCoordinate, Rotation);
        return Board.GetLetterAtCoordinate(newCoordinate);
    }

    public int Score
    {
        get
        {
            return Words.Sum(s =>s.Points);
        }
    }

    public int NumberOfWords => Words.Count();

    private IEnumerable<FoundWord> Words
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
