using System.Collections.Immutable;

namespace Moggle
{

public record EquationWord(string Text, ImmutableList<Coordinate> Path) : FoundWord(Text, Path)
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
