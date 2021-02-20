namespace Moggle
{

public record MathExpressionWord : FoundWord
{
    private MathExpressionWord(string text, int result) : base(text)
    {
        Result = result;
    }

    public static MathExpressionWord? TryCreate(string s)
    {
        var v = MathParser.Parser.GetExpressionValue(s);

        if (v.HasValue)
            return new MathExpressionWord(s, v.Value);

        return null;
    }

    public readonly int Result;

    /// <inheritdoc />
    protected override int CompareTo(FoundWord fw)
    {
        if (fw is MathExpressionWord mew)
            return Result.CompareTo(mew.Result);

        return base.CompareTo(fw);
    }

    /// <inheritdoc />
    public override string Display => $"{Result}: {Text}";

    /// <inheritdoc />
    public override string Comparison => Result.ToString();

    /// <inheritdoc />
    public override int Points => 1;

}

}
