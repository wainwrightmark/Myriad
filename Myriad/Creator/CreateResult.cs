using System.Collections.Generic;
using System.Text;

namespace Myriad.Creator;

public abstract record CreateResult
{
    public record SolvedGrid(NodeGrid Grid) : CreateResult;

    public record NextStates(IReadOnlyCollection<SolveState> States) : CreateResult;

    public record CantCreate(Rune StickingPoint) : CreateResult;
}