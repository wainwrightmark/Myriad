using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record MoggleState(
    MoggleBoard Board,
    Solver Solver,
    TimeSituation TimeSituation,
    ImmutableList<Coordinate> ChosenPositions,
    ImmutableSortedSet<FoundWord> FoundWords,
    ImmutableHashSet<FoundWord> DisabledWords,
    ImmutableList<FoundWord>? CheatWords,
    IMoggleGameMode LastGameMode,
    ImmutableDictionary<string, string> LastSettings)
{
    public static readonly MoggleState DefaultState =
        StartNewGame(
            WordList.LazyInstance,
            CenturyGameMode.Instance,
            ImmutableDictionary<string, string>.Empty,
            null
        );

    public static MoggleState StartNewGame(
        WordList wordList,
        IMoggleGameMode gameMode,
        ImmutableDictionary<string, string> settings,
        SavedGame? savedGame) => StartNewGame(
        new Lazy<WordList>(() => wordList),
        gameMode,
        settings,
        savedGame
    );

    public static MoggleState StartNewGame(
        Lazy<WordList> wordList,
        IMoggleGameMode gameMode,
        ImmutableDictionary<string, string> settings,
        SavedGame? savedGame)
    {
        var (board, solver, timeSituation) = gameMode.CreateGame(settings, wordList);

        ImmutableSortedSet<FoundWord> foundWords;

        if (savedGame == null)
            foundWords = ImmutableSortedSet<FoundWord>.Empty;
        else
            foundWords = savedGame.FoundWords.Select(solver.CheckLegal)
                .OfType<WordCheckResult.Legal>()
                .Select(x=>x.Word)
                .ToImmutableSortedSet()!;

        MoggleState newState = new(
            board,
            solver,
            timeSituation,
            ImmutableList<Coordinate>.Empty,
            foundWords,
            ImmutableHashSet<FoundWord>.Empty,
            null,
            gameMode,
            settings
        );

        return newState;
    }

    public MoveResult TryGetMoveResult(Coordinate coordinate)
    {
        if (TimeSituation is TimeSituation.Finished)
            return MoveResult.IllegalMove.Instance;

        if (TimeSituation.IsFinished)
            return new MoveResult.TimeElapsed(
                this with
                {
                    TimeSituation = TimeSituation.Finished.Instance,
                    ChosenPositions = ImmutableList<Coordinate>.Empty
                }
            );

        if (ChosenPositions.Any() && ChosenPositions.Last().Equals(coordinate))
        {
            return new MoveResult.WordAbandoned(
                this with { ChosenPositions = ImmutableList<Coordinate>.Empty }
            );
        }

        var index = ChosenPositions.LastIndexOf(coordinate);

        if (index >= 0)
        {
            var newChosenPositions = ChosenPositions.Take(index + 1).ToImmutableList();
                var word = string.Join(
                    "",
                    newChosenPositions.Select(GetLetterAtCoordinate).Select(x => x.WordText)
                );

                var animationWord    = Solver.CheckLegal(word).ToAnimationWord(true, word);

                return new MoveResult.MoveRetraced(
                this with { ChosenPositions = newChosenPositions },
                animationWord
            );
        }

        if (!ChosenPositions.Any() || ChosenPositions.Last().IsAdjacent(coordinate))
        {
            var newChosenPositions = ChosenPositions.Add(coordinate);

            var word = string.Join(
                "",
                newChosenPositions.Select(GetLetterAtCoordinate).Select(x => x.WordText)
            );

            var checkResult = Solver.CheckLegal(word);

            if (checkResult is WordCheckResult.Legal legal)
            {
                if (FoundWords.Contains(legal.Word))
                {
                    return new MoveResult.WordContinued(
                        this with { ChosenPositions = newChosenPositions },
                        checkResult.ToAnimationWord(true, word)
                    );
                }

                return new MoveResult.WordComplete(
                    this with
                    {
                        FoundWords = FoundWords.Add(legal.Word),
                        ChosenPositions = newChosenPositions
                    },
                    legal.Word
                );
            }

            return new MoveResult.WordContinued(
                this with { ChosenPositions = newChosenPositions },
                checkResult.ToAnimationWord(false, word)
            );
        }

        return MoveResult.IllegalMove.Instance;
    }

    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        //var newCoordinate = coordinate.Rotate(Board.MaxCoordinate, Rotation);
        return Board.GetLetterAtCoordinate(coordinate);
    }

    public int Score
    {
        get
        {
            return Words.Sum(s => s.Points);
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

    //public MoggleState Rotate(int amount)
    //{
    //    var newRotation = (Rotation + amount) % 4;
    //    var newBoard    = newRotation % 2 == 1 ? Board with { Columns = Board.Rows } : Board;
    //        return this with
    //        {
    //            Rotation = newRotation,
    //            ChosenPositions = ChosenPositions
    //            .Select(x => x.Rotate(Board.MaxCoordinate, amount))
    //            .ToImmutableList(),
    //            Board = newBoard
    //        };
    //    }
}

}
