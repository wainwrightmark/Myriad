using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle.Creator
{

public record SolveState(NodeGrid Grid, ImmutableList<Node> RemainingNodes)
{
    public static SolveState? ApplyAllSingleMoves(SolveState ss)
    {
        while (true)
        {
            var singleMoveNodes = ss.RemainingNodes
                .Select(node => (node, locations: node.FindPossibleLocations(ss.Grid)))
                .Where(x => x.locations.Count <= 1)
                .OrderBy(x => x.locations.Count)
                .ToList();

            if (!singleMoveNodes.Any())
                return ss;

            foreach (var (n, l) in singleMoveNodes)
            {
                if (l.Count == 0)
                    return null; //no possible move

                var coordinate = l.Single();

                if (!n.CanPlay(ss.Grid, coordinate))
                    return null;

                ss = ss.PlayMove(n, coordinate);
            }

            //Now that we've played a move, try again
        }
    }

    private SolveState PlayMove(Node node, Coordinate coordinate)
    {
        var newNodes = RemainingNodes.Remove(node);

        var oldSetAtCoordinate = Grid.Dictionary.TryGetValue(coordinate, out var s)
            ? s
            : ImmutableSortedSet<Node>.Empty;

        var newSet = oldSetAtCoordinate.Add(node);
        newSet = newSet.Add(node);

        var newGrid = new NodeGrid(
            Grid.Dictionary.SetItem(coordinate, newSet),
            Grid.MaxCoordinate
        );

        return new SolveState(newGrid, newNodes);
    }

    public CreateResult TrySolve()
    {
        if (!RemainingNodes.Any())
            return new CreateResult.SolvedGrid(Grid);

        var nextNode = RemainingNodes
            .Select(node => (node, locations: node.FindPossibleLocations(Grid)))
            .OrderBy(x => x.locations.Count)                             //Fewest locations
            .ThenBy(x => x.node.RootNodeGroup.RootNodes.Count)           //Fewest root nodes
            .ThenByDescending(x => x.node.RootNodeGroup.ConstraintScore) //Most constrained
            .First();

        if (nextNode.locations.Count == 0)
        {
            return CreateResult.CantCreate.Instance;
        }

        List<SolveState> nextStates = new();

        IOrderedEnumerable<Coordinate> orderedLocations =
            nextNode.locations.OrderByDescending(x => x.DistanceFromCentre(Grid.MaxCoordinate));

        if (nextNode.node.RootNodeGroup.RootNodes.Count == 1)
            orderedLocations =
                nextNode.locations.OrderByDescending(x => x.DistanceFromCentre(Grid.MaxCoordinate));
        else
            orderedLocations =
                nextNode.locations.OrderBy(x => x.DistanceFromCentre(Grid.MaxCoordinate));

        foreach (var coordinate in orderedLocations)
        {
            var next = PlayMove(nextNode.node, coordinate);
            next = ApplyAllSingleMoves(next);

            if (next != null)
                nextStates.Add(next);
        }

        if (nextStates.Any())
            return new CreateResult.NextStates(nextStates);

        return CreateResult.CantCreate.Instance;
    }
}

}
