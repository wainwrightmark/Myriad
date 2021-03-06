using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using MoreLinq;

namespace Moggle.Creator
{

public static class GridCreator
{
    public static NodeGrid CreateNodeGridFromText(string text, ILogger? logger, int msCancellation)
    {
        var words = GetAllWords(text);

        return CreateNodeGrid(words, logger, msCancellation);
    }

    public static IEnumerable<string> GetAllWords(string text)
    {
        var words = text.ToUpper()
            .Split(
                new[] { '\r', '\n', ' ', '\t' },
                StringSplitOptions.None | StringSplitOptions.RemoveEmptyEntries
            )
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(LettersOnly)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return words;

        string LettersOnly(string s) => new(s.Where(char.IsLetter).ToArray());
    }

    public static (NodeGrid grid, ImmutableList<string> words)? CreateGridForMostWords(
        ImmutableList<string> mustWords,
        ImmutableList<string> possibleWords,
        ILogger? logger,
        Coordinate maxCoordinate,
        CancellationToken cancellation)
    {
        var stack =
            new Stack<(string pw, Dictionary<Rune, BaseRuneMultiplicity> multiplicities, int sum)>(
                possibleWords.Select(
                        pw =>
                        {
                            var multiplicities =
                                CreateMultiplicities(mustWords.Append(pw))
                                    .ToDictionary(x => x.Rune);

                            var sum = multiplicities.Sum(x => x.Value.Multiplicity);

                            return (pw, multiplicities, sum);
                        }
                    )
                    .OrderByDescending(x => x.sum)
            );

        (NodeGrid grid, ImmutableList<string> words)? bestSoFar = null;

        void SetBest(NodeGrid grid, ImmutableList<string> words)
        {
            if (bestSoFar is null || bestSoFar.Value.words.Count < words.Count)
            {
                logger.LogInformation(
                    $"Found grid for {words.Count} words: {words.ToDelimitedString(", ")}"
                );

                bestSoFar = (grid, words);
            }
        }

        while (!cancellation.IsCancellationRequested
            && stack.Count + mustWords.Count > (bestSoFar?.words.Count ?? 0)
            && stack.TryPop(out var w))
        {
            var emptyGrid = new NodeGrid(
                ImmutableSortedDictionary<Coordinate, ImmutableSortedSet<Node>>.Empty,
                maxCoordinate
            );

            var words = mustWords.Add(w.pw);

            var nodes = CreateNodes(words, w.multiplicities.Values);

            var creator = new Creator();

            var r = creator.Create(
                new SolveState(emptyGrid, nodes.ToImmutableList()),
                cancellation,
                logger
            );

            if (r is not null)
            {
                SetBest(r, words);
                var remainingPossibles = stack.Select(x => x.pw).ToImmutableList();

                //We need to go deeper
                var newBest = CreateGridForMostWords(
                    words,
                    remainingPossibles,
                    logger,
                    maxCoordinate,
                    cancellation
                );

                if(newBest is not null)
                    SetBest(newBest.Value.grid, newBest.Value.words);
            }
        }

        return bestSoFar;
    }

    public static NodeGrid CreateNodeGrid(
        IEnumerable<string> allWords,
        ILogger? logger,
        int msCancellation)
    {
        var words = RemoveSubstrings(
                allWords.Select(x => x.ToUpper()),
                StringComparison.OrdinalIgnoreCase
            )
            .ToList();

        logger?.LogInformation(words.Count + " words");
        logger?.LogInformation(string.Join(", ", words));

        var runeMultiplicities = CreateMultiplicities(words).ToDictionary(x => x.Rune);

        var rmString = string.Join(", ", runeMultiplicities.Select(x => x.Value));

        logger?.LogInformation(rmString);

        var sw = Stopwatch.StartNew();

        while (true)
        {
            var nodes         = CreateNodes(words, runeMultiplicities.Values);
            var numberOfNodes = nodes.OfType<RootNode>().Count();

            var maxCoordinate = Coordinate.GetMaxCoordinateForSquareGrid(numberOfNodes);

            var emptyGrid = new NodeGrid(
                ImmutableSortedDictionary<Coordinate, ImmutableSortedSet<Node>>.Empty,
                maxCoordinate
            );

            var creator = new Creator();

            var r = creator.Create(
                new SolveState(emptyGrid, nodes.ToImmutableList()),
                new CancellationTokenSource(msCancellation).Token,
                logger
            );

            if (r is not null)
            {
                sw.Stop();

                logger?.LogInformation(
                    $"{numberOfNodes} cell Grid Found in " + sw.ElapsedMilliseconds
                                                           + $"ms after {creator.TriedGrids.Count} tries"
                );

                return r;
            }

            var mostConstrainedNode = nodes.Select(x => x.RootNodeGroup)
                .OrderByDescending(x => x.ConstraintScore)
                .First();

            var newNumber = mostConstrainedNode.RootNodes.Count + 1;

            logger?.LogInformation(
                $"No Grid found after {sw.ElapsedMilliseconds}ms, increasing {mostConstrainedNode.Rune} to {mostConstrainedNode.RootNodes.Count + 1} after {creator.TriedGrids.Count} tries"
            );

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

    private record BaseRuneMultiplicity(Rune Rune, int Multiplicity)
    {
        public RootNodeGroup Create()
        {
            var rnl = new RootNodeGroup(Rune);

            var list =
                Enumerable.Range(0, Multiplicity)
                    .Select(x => new RootNode($"0{Rune}{x}", Rune, rnl))
                    .ToImmutableList();

            rnl.RootNodes = list;
            return rnl;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Rune} {Multiplicity}";
    }

    private static IReadOnlyList<Node> CreateNodes(
        IEnumerable<string> words,
        IEnumerable<BaseRuneMultiplicity> baseRuneMultiplicities)
    {
        Dictionary<Rune, RootNodeGroup> runeDictionary =
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
