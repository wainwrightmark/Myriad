using System.Collections.Immutable;
using Fluxor;

namespace Moggle.States
{

public class CheatFeature : Feature<CheatState>
{
    /// <inheritdoc />
    public override string GetName() => "Cheat";

    /// <inheritdoc />
    protected override CheatState GetInitialState()
    {
        return new(false, false);
    }
}

}
