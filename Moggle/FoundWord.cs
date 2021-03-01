using System;

namespace Moggle
{
public abstract record FoundWord(string Text) : IComparable
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

    protected virtual int CompareTo(FoundWord fw) => string.Compare(Comparison, fw.Comparison, StringComparison.OrdinalIgnoreCase);
}


}
