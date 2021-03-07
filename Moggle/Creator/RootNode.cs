using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moggle.Creator
{

public class RootNode : Node
{
    /// <inheritdoc />
    public RootNode(string id, Rune rune, RootNodeGroup rootNodeGroup) :
        base(id, rune, rootNodeGroup) { }

    /// <inheritdoc />
    public override ISet<Coordinate> FindPossibleLocations(NodeGrid grid)
    {
        var nodeGridLocations = grid.GetNodeLocations();

        if (nodeGridLocations.TryGetValue(this, out var c))
            return new HashSet<Coordinate> { c };

        var              allowAnyUnusedCell = false;
        ISet<Coordinate> set      = new HashSet<Coordinate>();

        foreach (var cn in RootNodeGroup.Constraints.SelectMany(x => x))
        {
            if (nodeGridLocations.TryGetValue(cn, out var coordinate))
                set.Add(coordinate);
            else
                allowAnyUnusedCell = true; //This constraint has not been placed - it could be any unused cell
        }

        if (allowAnyUnusedCell)
            set = grid.GetAllUnusedLocations().Union(set);

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
                set = set.Where(
                        x => x.HasAtLeastXNeighbors(requiredAdjacentNodes, grid.MaxCoordinate)
                    )
                    .ToHashSet();
            }
        }

        return set;

        //TODO check alternatives if multiple root nodes?
    }

    /// <inheritdoc />
    public override bool CanPlay(NodeGrid grid, Coordinate coordinate)
    {
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
                if (!coordinate.HasAtLeastXNeighbors(requiredAdjacentNodes, grid.MaxCoordinate))//TODO do a bit better here
                    return false;
            }
        }

        return true;
    }
}

}
