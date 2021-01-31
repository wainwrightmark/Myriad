using System.Collections.Generic;

namespace Moggle.Creator
{

public abstract record CreateResult
{
    public record SolvedGrid(NodeGrid Grid) : CreateResult;

    public record NextStates(IReadOnlyCollection<SolveState> States) : CreateResult;

    public record CantCreate : CreateResult
    {
        private CantCreate() { }
        public static CantCreate Instance { get; } = new ();
    }
}

}
