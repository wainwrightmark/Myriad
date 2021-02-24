using System;
using Moggle.States;

namespace Moggle
{

public abstract record MoveResult
{
    public abstract record SuccessResult(MoggleState MoggleState) : MoveResult { }

    public abstract record FailResult : MoveResult;

    public record WordComplete(MoggleState MoggleState, FoundWord FoundWord) : SuccessResult(
        MoggleState
    )
    {
        /// <inheritdoc />
        public override AnimationWord AnimationWord => new(
            FoundWord.AnimationString,
            AnimationWord.WordType.Found
        );
    }

    public record WordContinued
        (MoggleState MoggleState, AnimationWord AnimationWord1) : SuccessResult(MoggleState)
    {
        /// <inheritdoc />
        public override AnimationWord AnimationWord => AnimationWord1;
    }

    public record WordAbandoned(MoggleState MoggleState) : SuccessResult(MoggleState)
    {
        /// <inheritdoc />
        public override AnimationWord? AnimationWord => null;
    }

    public record MoveRetraced
        (MoggleState MoggleState, AnimationWord AnimationWord1) : SuccessResult(MoggleState)
    {
        /// <inheritdoc />
        public override AnimationWord AnimationWord => AnimationWord1;
    }

    //public record TimeElapsed(MoggleState MoggleState) : SuccessResult(MoggleState)
    //{
    //    /// <inheritdoc />
    //    public override AnimationWord? AnimationWord => null;
    //}

    public record InvalidWord(AnimationWord AnimationWord1) : FailResult
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
                    WordType.Invalid         => basis * 1,
                    WordType.Illegal         => basis * 3,
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
