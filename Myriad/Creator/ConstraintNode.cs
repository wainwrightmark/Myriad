using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Myriad.Creator
{

public class ConstraintNode : Node
{
    /// <inheritdoc />
    public ConstraintNode(string id, Rune rune, RootNodeGroup rootNodeGroup) : base(
        id,
        rune,
        rootNodeGroup
    ) { }

    public IEnumerable<ConstraintNode> AdjacentNodes => _adjacentNodes;
    private readonly List<ConstraintNode> _adjacentNodes = new();

    public void AddAdjacent(ConstraintNode n) => _adjacentNodes.Add(n);

    public IEnumerable<ConstraintNode> ExclusiveNodes => _exclusiveNodes;
    private readonly List<ConstraintNode> _exclusiveNodes = new();

    public void AddExclusive(ConstraintNode n) => _exclusiveNodes.Add(n);

    public override ISet<Coordinate> FindPossibleLocations(NodeGrid grid)
    {
        var nodeGridLocations = grid.GetNodeLocations();

        if (nodeGridLocations.TryGetValue(this, out var c))
            return new HashSet<Coordinate>() { c };

        List<IReadOnlySet<Coordinate>> restrictions = new();

        HashSet<Coordinate> rootNodeLocations = nodeGridLocations
            .Where(x => x.Key.RootNodeGroup == RootNodeGroup)
            .Select(x => x.Value)
            .ToHashSet();

        if (rootNodeLocations.Count > RootNodeGroup.RootNodes.Count)
            return ImmutableHashSet<Coordinate>.Empty; //This is impossible
        else if (rootNodeLocations.Count < RootNodeGroup.RootNodes.Count)
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

        if (RootNodeGroup.RootNodes.Count == 1) //TODO why does this make it so much slower
        {
            var requiredAdjacentNodes =
                RootNodeGroup.Constraints.SelectMany(x => x)
                    .SelectMany(x => x.AdjacentNodes) //TODO check alternatives???
                    .Select(x => x.RootNodeGroup)
                    .Distinct()
                    .Count();

            if (requiredAdjacentNodes > 3)
            {
                finalSet = finalSet.Where(
                        x => x.HasAtLeastXNeighbors(requiredAdjacentNodes, grid.MaxCoordinate)
                    )
                    .ToHashSet();
            }
        }

        return finalSet;
    }

    /// <inheritdoc />
    public override bool CanPlay(NodeGrid grid, Coordinate coordinate)
    {
        if (grid.Dictionary.TryGetValue(coordinate, out var existingNodes) && existingNodes.Any())
        {
            if (existingNodes.First().RootNodeGroup != RootNodeGroup)
                return false;

            if (existingNodes.Overlaps(ExclusiveNodes))
                return false;
        }

        foreach (var constraintNode in AdjacentNodes)
        {
            if (grid.GetNodeLocations().TryGetValue(constraintNode, out var other))
                if (!other.IsAdjacent(coordinate))
                    return false;
        }

        if (RootNodeGroup.RootNodes.Count == 1)
        {
            var requiredAdjacentNodes =
                RootNodeGroup.Constraints.SelectMany(x => x)
                    .SelectMany(x => x.AdjacentNodes) //TODO check alternatives???
                    .Select(x => x.RootNodeGroup)
                    .Distinct()
                    .Count();

            if (requiredAdjacentNodes > 3)
            {
                if (!coordinate.HasAtLeastXNeighbors(requiredAdjacentNodes, grid.MaxCoordinate)
                ) //TODO do a bit better here
                    return false;
            }
        }

        return true;
    }
}

}
