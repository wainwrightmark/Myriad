namespace Moggle
{

public abstract record MoveResult
{
    public abstract record SuccessResult(MoggleState MoggleState) : MoveResult { }

    public abstract record FailResult : MoveResult;

    public record WordComplete(MoggleState MoggleState, FoundWord FoundWord) : SuccessResult(MoggleState) { }
    public record WordContinued(MoggleState MoggleState) : SuccessResult(MoggleState) { }
    public record WordAbandoned(MoggleState MoggleState) : SuccessResult(MoggleState) { }
    public record MoveRetraced(MoggleState MoggleState) : SuccessResult(MoggleState) { }
    public record TimeElapsed(MoggleState MoggleState) : SuccessResult(MoggleState) { }

    public record InvalidWord : FailResult
    {
        private InvalidWord() { }
        public static InvalidWord Instance { get; } = new();
    }

    public record IllegalMove : FailResult
    {
        private IllegalMove() { }
        public static IllegalMove Instance { get; } = new();
    }
}

}
