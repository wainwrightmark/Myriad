using System;

namespace Moggle
{

public record ExpressionWord : FoundWord
{
    private ExpressionWord(string text, int result) : base(text)
    {
        Result = result;
    }

    public static ExpressionWord? TryCreate(string s)
    {
        var v = MathParser.Parser.GetExpressionValue(s);

        if (v.HasValue)
            return new ExpressionWord(s, v.Value);

        return null;
    }

    public readonly int Result;

    /// <inheritdoc />
    protected override int CompareTo(FoundWord fw)
    {
        if (fw is ExpressionWord mew)
            return -Result.CompareTo(mew.Result);
        

        return base.CompareTo(fw);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <inheritdoc />
    protected override Type EqualityContract { get; } = typeof(FoundWord);

    /// <inheritdoc />
    public override string Display => $"{Result}: {Text}";

    /// <inheritdoc />
    public override string Comparison => Result.ToString();

    /// <inheritdoc />
    public override string AnimationString => Result.ToString();

    /// <inheritdoc />
    public override int Points => 1;

}

}
