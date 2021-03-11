using System;
using System.Collections.Generic;
using System.Text;

namespace Moggle.Creator
{

public abstract class Node : IEquatable<Node>, IComparable<Node>, IComparable
{
    protected Node(string id, Rune rune, RootNodeGroup rootNodeGroup)
    {
        Id           = id;
        Rune         = rune;
        RootNodeGroup = rootNodeGroup;
    }

    public RootNodeGroup RootNodeGroup { get; }

    public string Id { get; }

    public Rune Rune { get; }

    public abstract ISet<Coordinate> FindPossibleLocations(NodeGrid grid);

    public abstract bool CanPlay(NodeGrid grid, Coordinate coordinate);

    /// <inheritdoc />
    public override string ToString() => Id;

    /// <inheritdoc />
    public int CompareTo(object? obj)
    {
        if (obj is Node n)
            return CompareTo(n);

        return 0;
    }

    /// <inheritdoc />
    public bool Equals(Node? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id == other.Id;
    }

    /// <inheritdoc />
    public int CompareTo(Node? other)
    {
        if (other is null)
            return 1;

        // ReSharper disable once StringCompareToIsCultureSpecific
        return Id.CompareTo(other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj is Node n && Id.Equals(n.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Node? left, Node? right) => Equals(left, right);

    public static bool operator !=(Node? left, Node? right) => !Equals(left, right);
}

}
