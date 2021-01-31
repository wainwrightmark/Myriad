using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Moggle.Creator
{

public class Creator
{
    public NodeGrid? Create(SolveState start, CancellationToken ct, ILogger logger)
    {
        SolveStates.Add(start);

        var r = Solve(ct, logger);

        return r;
    }

    private NodeGrid? Solve(CancellationToken ct, ILogger logger)
    {
        while (!ct.IsCancellationRequested && SolveStates.TryTake(out var ss)) //TODO combine depth and breadth
        {
            var r = ss.TrySolve();

            switch (r)
            {
                case CreateResult.CantCreate _: break;
                case CreateResult.NextStates nextStates:
                {
                    foreach (var ns in nextStates.States)
                    {
                        if(TriedGrids.Add(ns.Grid))
                            SolveStates.Add(ns);
                    }

                    break;
                }
                case CreateResult.SolvedGrid solvedGrid: return solvedGrid.Grid;
                default: throw new ArgumentOutOfRangeException(nameof(r));
            }
        }

        if (SolveStates.Any())
        {
                logger.LogInformation($"Giving up with {SolveStates.First().RemainingNodes.Count} nodes remaining to place");
                logger.LogInformation(SolveStates.First().Grid.ToString());
        }



        return null;
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
