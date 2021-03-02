using System;
using System.Collections.Immutable;
using System.Linq;
using Moggle.States;

namespace Moggle
{

public abstract record MoveResult
{
    public abstract record SuccessResult(ImmutableList<Coordinate> NewCoordinates) : MoveResult { }

    public abstract record FailResult : MoveResult;

    public record WordComplete
        (FoundWord FoundWord, ImmutableList<Coordinate> NewCoordinates) : SuccessResult(
            NewCoordinates
        )
    {
        /// <inheritdoc />
        public override AnimationWord AnimationWord => new(
            FoundWord.AnimationString,
            AnimationWord.WordType.Found
        );
    }

    public record WordContinued
        (AnimationWord AnimationWord1, ImmutableList<Coordinate> NewCoordinates) : SuccessResult(
            NewCoordinates
        )
    {
        /// <inheritdoc />
        public override AnimationWord AnimationWord => AnimationWord1;
    }

    public record WordAbandoned() : SuccessResult(ImmutableList<Coordinate>.Empty)
    {
        /// <inheritdoc />
        public override AnimationWord? AnimationWord => null;
    }

    public record MoveRetraced
        (ImmutableList<Coordinate> NewCoordinates, AnimationWord AnimationWord1) : SuccessResult(
            NewCoordinates
        )
    {
        /// <inheritdoc />
        public override AnimationWord AnimationWord => AnimationWord1;
    }

    public record IllegalMove : FailResult
    {
        private IllegalMove() { }
        public static IllegalMove Instance { get; } = new();

        /// <inheritdoc />
        public override AnimationWord? AnimationWord => null;
    }

    public abstract AnimationWord? AnimationWord { get; }

    public static MoveResult GetMoveResult(
        Coordinate coordinate,
        ChosenPositionsState chosenPositionsState,
        MoggleBoard moggleBoard,
        Solver solver,
        FoundWordsState foundWordsState)
    {
        var chosenPositions = chosenPositionsState.ChosenPositions;

        if (chosenPositions.Any() && (chosenPositions.Last().Equals(coordinate)
                                   || chosenPositions.First().Equals(coordinate)))
        {
            return new WordAbandoned();
        }

        var index = chosenPositions.LastIndexOf(coordinate);

        if (index >= 0)
        {
            var newChosenPositions = chosenPositions.Take(index + 1).ToImmutableList();

            var word = string.Join(
                "",
                newChosenPositions.Select(moggleBoard.GetLetterAtCoordinate).Select(x => x.WordText)
            );

            var animationWord = solver.CheckLegal(word).ToAnimationWord(true, word);

            return new MoveRetraced(
                newChosenPositions,
                animationWord
            );
        }

        if (!chosenPositions.Any() || chosenPositions.Last().IsAdjacent(coordinate))
        {
            var newChosenPositions = chosenPositions.Add(coordinate);

            var word = string.Join(
                "",
                newChosenPositions.Select(moggleBoard.GetLetterAtCoordinate).Select(x => x.WordText)
            );

            var checkResult = solver.CheckLegal(word);

            if (checkResult is WordCheckResult.Legal legal)
            {
                if (foundWordsState.Data.WordIsFound(legal.Word))
                {
                    return new WordContinued(
                        checkResult.ToAnimationWord(true, word),
                        newChosenPositions
                    );
                }

                return new WordComplete(
                    legal.Word,
                    newChosenPositions
                );
            }

            return new WordContinued(
                checkResult.ToAnimationWord(false, word),
                newChosenPositions
            );
        }

        return IllegalMove.Instance;
    }
}

public record AnimationWord(string Text, AnimationWord.WordType Type)
{
    public int LingerDuration
    {
        get
        {
            const int basis = 1000;

            return Type switch
            {
                WordType.Found           => basis * 10,
                WordType.PreviouslyFound => basis * 5,
                WordType.Invalid         => basis * 2,
                WordType.Illegal         => basis * 4,
                _                        => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public enum WordType
    {
        Found,
        PreviouslyFound,
        Invalid,
        Illegal
    }
}

}
