namespace Moggle
{

public abstract record WordCheckResult(bool IsLegal)
{
    public record Invalid : WordCheckResult
    {
        private Invalid() : base(false) { }
        public static Invalid Instance { get; } = new();

        /// <inheritdoc />
        public override AnimationWord ToAnimationWord(bool previouslyFound, string text)
        {
            return new (text, AnimationWord.WordType.Invalid);
        }
    }

    public record Legal(FoundWord Word) : WordCheckResult(true)
    {
        /// <inheritdoc />
        public override AnimationWord ToAnimationWord(bool previouslyFound, string text)
        {
            return new (
                Word.AnimationString,
                previouslyFound
                    ? AnimationWord.WordType.PreviouslyFound
                    : AnimationWord.WordType.Found
            );
        }
    }

    public record Illegal(FoundWord Word) : WordCheckResult(false)
    {
        /// <inheritdoc />
        public override AnimationWord ToAnimationWord(bool previouslyFound, string text)
        {
            return new (Word.AnimationString, AnimationWord.WordType.Illegal);
        }
    }

    public abstract AnimationWord ToAnimationWord(bool previouslyFound, string text);
}

}
