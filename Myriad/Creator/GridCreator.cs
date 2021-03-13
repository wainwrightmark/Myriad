using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Myriad.Creator
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

        static string LettersOnly(string s) => new(s.Where(char.IsLetter).ToArray());
    }

        private static NodeGrid CreateEmptyGrid(Coordinate maxCoordinate) => new (
                ImmutableSortedDictionary<Coordinate, ImmutableSortedSet<Node>>.Empty,
                maxCoordinate

            )!;


        private static NodeGrid? TryCreate(
                    ImmutableList<string> words,
                    int maxTries,
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
                    maxTries,
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

                            break;
                        }
                    case NodeGridCreateResult.OtherFailure _:
                    {
                        return null;
                    }
                    case NodeGridCreateResult.Success success:
                    {
                        return success.NodeGrid;
                    }
                    default: throw new ArgumentOutOfRangeException(nameof(result));
                }
            }

            return null;
        }

        public static (NodeGrid grid, ImmutableList<string> words)? CreateGridForMostWords(
        ImmutableList<string> words,
        int maxTries,
        ILogger? logger,
        Stopwatch stopwatch,
        Coordinate maxCoordinate,
        CancellationToken cancellation)
        {
            var maxSize = (maxCoordinate.Column + 1) * (maxCoordinate.Row + 1);
            var possibleCombinations = new List<ImmutableList<string>>();


            void GetCombinations(ImmutableList<string> mustWords, ImmutableDictionary<char, int> multiplicitiesSoFar, ImmutableList<string> possibleWords)
            {
                var currentRemainingWords = possibleWords;

                while (currentRemainingWords.Any())
                {
                    var word = currentRemainingWords[0];
                    currentRemainingWords = currentRemainingWords.RemoveAt(0);

                    var theseWords             = mustWords.Add(word);
                    var thisWordMultiplicities = multiplicitiesSoFar;

                    foreach (var group in word.GroupBy(x=>x))
                    {
                        var c = group.Count();
                        var current = thisWordMultiplicities.TryGetValue(group.Key, out var v) ? v : 0;
                        if(c > current)
                            thisWordMultiplicities = thisWordMultiplicities.SetItem(group.Key, c);
                    }

                    var sum = thisWordMultiplicities.Sum(x => x.Value);

                    if (sum <= maxSize)
                    {
                        possibleCombinations.Add(theseWords);
                        GetCombinations(theseWords,thisWordMultiplicities, currentRemainingWords);
                    }

                    if (mustWords.Count == 0)
                    {
                        logger?.LogInformation($"{stopwatch.Elapsed}: {word} eliminated. {possibleCombinations.Count} options found. {currentRemainingWords.Count} remain.");
                    }
                }
            }

            GetCombinations(ImmutableList<string>.Empty, ImmutableDictionary<char, int>.Empty, words.OrderByDescending(x=>x.Length).ToImmutableList());

            var groups = possibleCombinations.GroupBy(x => x.Count).OrderBy(x=>x.Key);


            if (logger is not null)
            {
                logger.LogInformation($"{stopwatch.Elapsed}: {possibleCombinations.Count} possible combinations found");

                foreach (var group in groups)
                    logger.LogInformation(
                        $"{stopwatch.Elapsed}: {group.Count()} possible combinations of size {@group.Key} found"
                    );
            }


            var orderedCombinations = possibleCombinations.OrderByDescending(x => x.Count)
                .ThenByDescending(x => x.Sum(y => y.Length)).ToList();

            int? lastComboWords = null;

            foreach (var wordList in orderedCombinations)
            {
                if(wordList.Count != lastComboWords)
                {
                    logger.LogInformation($"{stopwatch.Elapsed}: Checking {wordList.Count}");
                    lastComboWords = wordList.Count;
                }
                var multiplicities =
                CreateMultiplicities(wordList)
                                       .ToImmutableDictionary(x => x.Rune);

                var grid = TryCreate(
                    wordList,
                    maxTries,
                    maxCoordinate,
                    multiplicities,
                    logger,
                    cancellation
                );

                if (grid is not null)
                    return (grid, wordList);
            }

            return null;
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
