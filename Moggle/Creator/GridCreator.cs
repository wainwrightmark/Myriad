﻿using System;
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

        private static NodeGrid CreateEmptyGrid(Coordinate maxCoordinate) => new (
                ImmutableSortedDictionary<Coordinate, ImmutableSortedSet<Node>>.Empty,
                maxCoordinate

            )!;


        private static NodeGrid? TryCreate(
                    ImmutableList<string> words,
                    Coordinate maxCoordinate,
                    ImmutableDictionary<Rune, BaseRuneMultiplicity> dictionary,
                    ILogger? logger,
                    CancellationToken cancellation)
        {
            var totalCellCount = (maxCoordinate.Column + 1) * (maxCoordinate.Row + 1);


            while (
                dictionary.Sum(x => x.Value.Multiplicity) <= totalCellCount
             && !cancellation.IsCancellationRequested)
            {
                var nodes = CreateNodes(words, dictionary.Values);
                var creator = new Creator();

                var result =
                creator.Create(
                    new SolveState(CreateEmptyGrid(maxCoordinate), nodes.ToImmutableList()),
                    logger,
                    10,
                    cancellation

                );

                switch (result)
                {
                    case NodeGridCreateResult.CouldNotPlaceFailure couldNotPlaceFailure:
                        {
                            var currentAmount = dictionary[couldNotPlaceFailure.Rune].Multiplicity;
                            var newAmount     = currentAmount + 1;

                            dictionary = dictionary.SetItem(
                                couldNotPlaceFailure.Rune,
                                new BaseRuneMultiplicity(
                                    couldNotPlaceFailure.Rune,
                                    newAmount
                                )
                            );

                            var totalMultiplicity = dictionary.Sum(x => x.Value.Multiplicity);

                            //logger.LogInformation($"Could not find grid for {words.Count} words with {totalMultiplicity} cells. Increasing {couldNotPlaceFailure.Rune} multiplicity to {newAmount}");

                            break;
                        }
                    case NodeGridCreateResult.OtherFailure _:
                    {
                        //logger?.LogTrace($"Could not find any grid for {words.ToDelimitedString(", ")}");

                        return null;
                    }
                    case NodeGridCreateResult.Success success: return success.NodeGrid;
                    default: throw new ArgumentOutOfRangeException(nameof(result));
                }
            }

            //logger?.LogInformation($"Could not find any grid for {words.ToDelimitedString(", ")}");

            return null;
        }

        public static (NodeGrid grid, ImmutableList<string> words)? CreateGridForMostWords(
        ImmutableList<string> mustWords,
        ImmutableList<string> possibleWords,
        ILogger? logger,
        Stopwatch stopwatch,
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
            if (bestSoFar is null ||
                bestSoFar.Value.words.Count < words.Count ||
                (bestSoFar.Value.words.Count == words.Count
              && bestSoFar.Value.words.Sum(x => x.Length) < words.Sum(x => x.Length)))
            {
                logger?.LogInformation(
                    $"{stopwatch.Elapsed}: Found grid for {words.Count} of {mustWords.Count + possibleWords.Count} words, {words.Sum(x => x.Length)} letters: {words.ToDelimitedString(", ")}, grid: {grid}"
                );

                bestSoFar = (grid, words);
            }
        }

        while (!cancellation.IsCancellationRequested
            && stack.Count + mustWords.Count > (bestSoFar?.words.Count ?? 0)
            && stack.TryPop(out var w))
        {
            var words = mustWords.Add(w.pw);

            var multiplicities = CreateMultiplicities(words).ToImmutableDictionary(x=>x.Rune);

            var nodeGrid = TryCreate(words, maxCoordinate, multiplicities, logger, cancellation);

            if (nodeGrid is not null)
            {
                SetBest(nodeGrid, words);
                var remainingPossibles = stack.Select(x => x.pw).ToImmutableList();

                //We need to go deeper
                var newBest = CreateGridForMostWords(
                    words,
                    remainingPossibles,
                    logger,
                    stopwatch,
                    maxCoordinate,
                    cancellation
                );

                if (newBest is not null)
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
                logger,
                100000,
                new CancellationTokenSource(msCancellation).Token
            );

            if (r is NodeGridCreateResult.Success success)
            {
                sw.Stop();

                logger?.LogInformation(
                    $"{numberOfNodes} cell Grid Found in " + sw.ElapsedMilliseconds
                                                           + $"ms after {creator.TriedGrids.Count} tries"
                );

                return success.NodeGrid;
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