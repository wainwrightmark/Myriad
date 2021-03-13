using System.Collections.Immutable;
using Fluxor;

namespace Myriad.States
{

public class SolverFeature : Feature<Solver>
{
    /// <inheritdoc />
    public override string GetName() => "Solver";

    /// <inheritdoc />
    protected override Solver GetInitialState()
    {
        return CenturyGameMode.Instance.CreateSolver(
                ImmutableDictionary<string, string>.Empty,
                WordList.LazyInstance
            )
            ;
    }
}

}
