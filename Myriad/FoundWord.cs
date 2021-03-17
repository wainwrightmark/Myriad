using System;
using System.Collections.Immutable;
using Myriad.MathParser;

namespace Myriad
{
public abstract record FoundWord(string Text, ImmutableList<Coordinate> Path) : IComparable
{
    public abstract string Display { get; }
    public abstract string Comparison { get; }

    /// <summary>
    /// The string that appears briefly after a word is found
    /// </summary>
    public abstract string AnimationString { get; }

    public abstract int Points { get; }

    /// <inheritdoc />
    public virtual bool Equals(FoundWord? other) =>
        other is not null && Comparison.Equals(other.Comparison);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var r = Comparison.GetHashCode();

        return r;
    }

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is FoundWord fw)
            return CompareTo(fw);

        return 0;
    }

    protected virtual int CompareTo(FoundWord fw) => String.Compare(Comparison, fw.Comparison, StringComparison.OrdinalIgnoreCase);

    public static FoundWord Create(string s, ImmutableList<Coordinate> path)
    {
        var ew = ExpressionWord.TryCreate(s, path);

        if (ew is not null)
            return ew;

        if (Parser.IsValidEquation(s))
            return new EquationWord(s, path);

        return new StringWord(s, path);
    }
}


}
