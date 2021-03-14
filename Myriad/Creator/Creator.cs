using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Myriad.Creator
{

public abstract record NodeGridCreateResult
{
    public record Success(NodeGrid NodeGrid) : NodeGridCreateResult;

    public record CouldNotPlaceFailure(Rune Rune) : NodeGridCreateResult;

    public record OtherFailure : NodeGridCreateResult;
}

public class Creator
{
    public NodeGridCreateResult Create(
        SolveState start,
        ILogger logger,
        int? maxTries,
        CancellationToken ct)
    {
        SolveStates.Add(start);

        var r = Solve(logger, maxTries, ct);

        return r;
    }

    private NodeGridCreateResult Solve(ILogger logger, int? maxTries, CancellationToken ct)
    {
        var tries              = 0;
        var stickingPointRunes = new ConcurrentDictionary<Rune, int>();

        while (!ct.IsCancellationRequested && SolveStates.TryTake(out var ss)
        ) //TODO combine depth and breadth
        {
            var r = ss.TrySolve();

            switch (r)
            {
                case CreateResult.CantCreate cc:
                {
                    stickingPointRunes.AddOrUpdate(cc.StickingPoint, 1, (_, c) => c + 1);

                    tries++;

                    if (maxTries is null || tries > maxTries)
                    {
                        logger.LogInformation(
                            $"giving up after {tries} tries of {maxTries}"
                        );

                        return new NodeGridCreateResult.CouldNotPlaceFailure(
                            stickingPointRunes.OrderByDescending(x => x.Value).First().Key
                        );
                    }

                    break;
                }
                case CreateResult.NextStates nextStates:
                {
                    foreach (var ns in nextStates.States)
                    {
                        if (TriedGrids.Add(ns.Grid))
                            SolveStates.Add(ns);
                    }

                    break;
                }
                case CreateResult.SolvedGrid solvedGrid:
                {
                    logger?.LogInformation(
                        $"{solvedGrid.Grid.Dictionary.Count} node grid found after {tries} tries"
                    );

                    return new NodeGridCreateResult.Success(solvedGrid.Grid);
                }

                default: throw new ArgumentOutOfRangeException(nameof(r));
            }
        }

        if (SolveStates.Any())
        {
            logger?.LogInformation(
                $"Giving up with {SolveStates.First().RemainingNodes.Count} nodes remaining to place"
            );

            logger?.LogInformation(SolveStates.First().Grid.ToString());
        }

        logger.LogInformation($"giving up after {tries} tries");

        if (stickingPointRunes.Any())
            return new NodeGridCreateResult.CouldNotPlaceFailure(
                stickingPointRunes.OrderByDescending(x => x.Value).First().Key
            );

        return new NodeGridCreateResult.OtherFailure();
    }

    public HashSet<NodeGrid> TriedGrids = new();

    public ConcurrentPriorityQueue<SolveState> SolveStates = new(SolveStateComparer.Instance);

    private record SolveStateComparer : IComparer<SolveState>
    {
        private SolveStateComparer() { }
        public static SolveStateComparer Instance { get; } = new();

        /// <inheritdoc />
        public int Compare(SolveState? x, SolveState? y)
        {
            if (x is null && y is null)
                return 0;

            if (x is null)
                return -1;

            if (y is null)
                return 1;

            return x.RemainingNodes.Count.CompareTo(y.RemainingNodes.Count) * -1;
        }
    }
}

}
