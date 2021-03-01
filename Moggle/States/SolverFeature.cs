using System.Collections.Immutable;
using Fluxor;

namespace Moggle.States
{

public class SolverFeature : Feature<Solver>
{
    /// <inheritdoc />
    public override string GetName() => "Solver";

    /// <inheritdoc />
    protected override Solver GetInitialState()
    {
            return CenturyGameMode.Instance.CreateGame(
                    ImmutableDictionary<string, string>.Empty,
                    WordList.LazyInstance
                )
                .Solver;
        }
}

}
