using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Moggle.Creator
{

public static class GridCreator
{
    public static NodeGrid CreateNodeGrid(IEnumerable<string> allWords)
    {
        var words = RemoveSubstrings(
                allWords.Select(x => x.ToUpper()),
                StringComparison.OrdinalIgnoreCase
            )
            .ToList();

        var runeMultiplicities = CreateMultiplicities(words).ToDictionary(x => x.Rune);

        while (true)
        {
            var nodes         = CreateNodes(words, runeMultiplicities.Values);
            var numberOfNodes = nodes.OfType<RootNode>().Count();

            var maxCoordinate = Coordinate.GetMaxCoordinateForSquareGrid(numberOfNodes);

            var emptyGrid = new NodeGrid(
                ImmutableSortedDictionary<Coordinate, ImmutableSortedSet<Node>>.Empty,
                maxCoordinate
            );

            var triedGrids = new HashSet<NodeGrid>();

            var r = FindFilledGrids(emptyGrid, triedGrids, nodes.ToImmutableList())
                .FirstOrDefault();

            if (r is not null)
                return r;

            var mostConstrainedNode = nodes.Select(x => x.RootNodeList)
                .OrderByDescending(x => x.ConstraintScore)
                .First();

            var newNumber = mostConstrainedNode.RootNodes.Count + 1;

            runeMultiplicities[mostConstrainedNode.Rune] =
                new BaseRuneMultiplicity(mostConstrainedNode.Rune, newNumber);
        }
    }

    private static IEnumerable<BaseRuneMultiplicity> CreateMultiplicities(IEnumerable<string> words)
    {
        return
            words.SelectMany(word => word.EnumerateRunes().GroupBy(x => x))
                .GroupBy(x => x.Key)
                .Select(x => new BaseRuneMultiplicity(x.Key, x.Max(y => y.Count())));
    }

    private static IEnumerable<string> RemoveSubstrings(
        IEnumerable<string> words,
        StringComparison comparison)
    {
        //return words;
        HashSet<string> usedWords = new();

        foreach (var w in words.Distinct().OrderByDescending(x => x.Length))
        {
            if (usedWords.Any(uw => uw.Contains(w, comparison)))
                continue;

            if (!usedWords.Add(w))
                continue;

            yield return w;
        }
    }

    private static IEnumerable<NodeGrid> FindFilledGrids(
        NodeGrid grid,
        ISet<NodeGrid> triedGrids,
        ImmutableList<Node> remainingNodes)
    {
        if (!triedGrids.Add(grid))
            yield break;

        if (!remainingNodes.Any())
        {
            yield return grid;

            yield break;
        }

        var nextNode = remainingNodes
            .Select(node => (node, locations: node.FindPossibleLocations(grid)))
            .OrderBy(x => x.locations.Count)
            .ThenByDescending(x => x.node.RootNodeList.ConstraintScore)
            .First();

        if (nextNode.locations.Count == 0)
        {
            yield break; //This is impossible
        }

        var newRemainingNodes = remainingNodes.Remove(nextNode.node);

        foreach (var nextNodeLocation in nextNode.locations)
        {
            var oldSetAtCoordinate = grid.Dictionary.TryGetValue(nextNodeLocation, out var s)
                ? s
                : ImmutableSortedSet<Node>.Empty;

            var newSet = oldSetAtCoordinate.Add(nextNode.node);

            var newGrid = new NodeGrid(
                grid.Dictionary.SetItem(nextNodeLocation, newSet),
                grid.MaxCoordinate
            );

            foreach (var a in FindFilledGrids(newGrid, triedGrids, newRemainingNodes))
            {
                yield return a;
            }
        }
    }

    private record BaseRuneMultiplicity(Rune Rune, int Multiplicity)
    {
        public RootNodeList Create()
        {
            var rnl = new RootNodeList(Rune);

            var list =
                Enumerable.Range(0, Multiplicity)
                    .Select(x => new RootNode($"0{Rune}{x}", Rune, rnl))
                    .ToImmutableList();

            rnl.RootNodes = list;
            return rnl;
        }
    }

    private static IReadOnlyList<Node> CreateNodes(
        IEnumerable<string> words,
        IEnumerable<BaseRuneMultiplicity> baseRuneMultiplicities)
    {
        Dictionary<Rune, RootNodeList> runeDictionary =
            baseRuneMultiplicities.ToDictionary(
                x => x.Rune,
                x => x.Create()
            );

        List<Node> allNodes = runeDictionary.Values
            .SelectMany(x => x.RootNodes)
            .OfType<Node>()
            .Distinct()
            .ToList();

        var i = 1;

        foreach (var word in words.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            List<ConstraintNode> wordNodes = new();

            foreach (var rune in word.EnumerateRunes())
            {
                var id   = i.ToString() + rune + wordNodes.Count;
                var rnl  = runeDictionary[rune];
                var node = new ConstraintNode(id, rune, rnl);

                if (wordNodes.Any())
                {
                    var previous = wordNodes.Last();
                    previous.AddAdjacent(node);
                    node.AddAdjacent(previous);
                }

                wordNodes.Add(node);
            }

            foreach (var group in wordNodes.GroupBy(x => x.Rune))
            {
                var thisWordNodes = group.ToImmutableList();

                foreach (var constraintNode in thisWordNodes)
                foreach (var o in thisWordNodes.Where(x => x != constraintNode))
                    constraintNode.AddExclusive(o);

                if (runeDictionary.TryGetValue(group.Key, out var rootNodeList))
                    rootNodeList.AddConstraintNode(thisWordNodes);
            }

            allNodes.AddRange(wordNodes);
            i++;
        }

        return allNodes;
    }
}

}
