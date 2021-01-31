using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Moggle.Creator
{

public class ConstraintNode : Node
{
    /// <inheritdoc />
    public ConstraintNode(string id, Rune rune, RootNodeList rootNodeList) : base(
        id,
        rune,
        rootNodeList
    ) { }

    public IEnumerable<ConstraintNode> AdjacentNodes => _adjacentNodes;
    private readonly List<ConstraintNode> _adjacentNodes = new();

    public void AddAdjacent(ConstraintNode n) => _adjacentNodes.Add(n);

    public IEnumerable<ConstraintNode> ExclusiveNodes => _exclusiveNodes;
    private readonly List<ConstraintNode> _exclusiveNodes = new();

    public void AddExclusive(ConstraintNode n) => _exclusiveNodes.Add(n);

    public override IReadOnlySet<Coordinate> FindPossibleLocations(NodeGrid grid)
    {
        var nodeGridLocations = grid.GetNodeLocations();

        if (nodeGridLocations.TryGetValue(this, out var c))
            return new HashSet<Coordinate>() { c };

        List<IReadOnlySet<Coordinate>> restrictions = new();

        HashSet<Coordinate> rootNodeLocations = nodeGridLocations
            .Where(x => x.Key.RootNodeList == RootNodeList)
            .Select(x => x.Value)
            .ToHashSet();

        if (rootNodeLocations.Count > RootNodeList.RootNodes.Count)
            return ImmutableHashSet<Coordinate>.Empty; //This is impossible
        else if (rootNodeLocations.Count < RootNodeList.RootNodes.Count)
            rootNodeLocations.UnionWith(grid.GetAllUnusedLocations());

        restrictions.Add(rootNodeLocations);

        foreach (var adjacentNode in _adjacentNodes)
        {
            if (nodeGridLocations.TryGetValue(adjacentNode, out var coordinate))
                restrictions.Add(coordinate.GetAdjacentCoordinates(grid.MaxCoordinate).ToHashSet());
        }

        var resultSet = restrictions.Aggregate<IEnumerable<Coordinate>>((x, y) => x.Intersect(y));

        var finalSet = resultSet.ToHashSet();

        foreach (var constraintNode in ExclusiveNodes)
        {
            if (nodeGridLocations.TryGetValue(constraintNode, out var coordinate))
                finalSet.Remove(coordinate);
        }

        return finalSet;
    }
}

public class RootNodeList
{
    public RootNodeList(Rune rune) => Rune = rune;

    public ImmutableList<RootNode> RootNodes { get; set; }
    public Rune Rune { get; }

    public IReadOnlyList<IReadOnlyList<ConstraintNode>> Constraints => _constraintNodes;

    private readonly List<IReadOnlyList<ConstraintNode>> _constraintNodes = new();

    public void AddConstraintNode(IReadOnlyList<ConstraintNode> constraintNodes) =>
        _constraintNodes.Add(constraintNodes);

    public double? ConstraintScore => ((double)_constraintNodes.Count) / RootNodes.Count;
}

public class RootNode : Node
{
    /// <inheritdoc />
    public RootNode(string id, Rune rune, RootNodeList rootNodeList) :
        base(id, rune, rootNodeList) { }

    /// <inheritdoc />
    public override IReadOnlySet<Coordinate> FindPossibleLocations(NodeGrid grid)
    {
        var nodeGridLocations = grid.GetNodeLocations();

        if (nodeGridLocations.TryGetValue(this, out var c))
            return new HashSet<Coordinate> { c };

        var allowAll = false;
        var set      = new HashSet<Coordinate>();

        foreach (var cn in RootNodeList.Constraints.SelectMany(x => x))
        {
            if (nodeGridLocations.TryGetValue(cn, out var coordinate))
                set.Add(coordinate);
            else
                allowAll = true;
        }

        //TODO maybe check constraints
        if (allowAll)
            return grid.GetAllUnusedLocations().Union(set);
        else
            return set;
    }
}

public abstract class Node : IEquatable<Node>, IComparable<Node>, IComparable
{
    protected Node(string id, Rune rune, RootNodeList rootNodeList)
    {
        Id           = id;
        Rune         = rune;
        RootNodeList = rootNodeList;
    }

    public RootNodeList RootNodeList { get; }

    public string Id { get; }

    public Rune Rune { get; }

    public abstract IReadOnlySet<Coordinate> FindPossibleLocations(NodeGrid grid);

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
        if (ReferenceEquals(null, other))
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
