using System;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle.States
{

public record MoggleState(//TODO move each of these to become its own object
    MoggleBoard Board,
    Solver Solver,
    ImmutableList<Coordinate> ChosenPositions,
    ImmutableSortedSet<FoundWord> FoundWords)
{
    public static readonly MoggleState DefaultState =
        StartNewGame(
            WordList.LazyInstance,
            CenturyGameMode.Instance,
            ImmutableDictionary<string, string>.Empty
        );

    public static MoggleState StartNewGame(
        WordList wordList,
        IMoggleGameMode gameMode,
        ImmutableDictionary<string, string> settings) => StartNewGame(
        new Lazy<WordList>(() => wordList),
        gameMode,
        settings
    );

    public static MoggleState StartNewGame(
        Lazy<WordList> wordList,
        IMoggleGameMode gameMode,
        ImmutableDictionary<string, string> settings)
    {
        var (board, solver) = gameMode.CreateGame(settings, wordList);

        MoggleState newState = new(
            board,
            solver,
            ImmutableList<Coordinate>.Empty,
            ImmutableSortedSet<FoundWord>.Empty
        );

        return newState;
    }

    public MoveResult TryGetMoveResult(Coordinate coordinate)
    {
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

            var animationWord = Solver.CheckLegal(word).ToAnimationWord(true, word);

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
        return Board.GetLetterAtCoordinate(coordinate);
    }
}

}
