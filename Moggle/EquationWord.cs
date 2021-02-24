namespace Moggle
{

public record EquationWord(string Text) : FoundWord(Text)
{
    /// <inheritdoc />
    public override string Display => Text;

    /// <inheritdoc />
    public override string Comparison => Text;

    /// <inheritdoc />
    public override string AnimationString => Text;

    /// <inheritdoc />
    public override int Points => 1;

}

}
