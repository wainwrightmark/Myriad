using System.Collections.Immutable;
using Fluxor;

namespace Moggle
{

public class UIStateFeature : Feature<UIState>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "UIState";
    }

    /// <inheritdoc />
    protected override UIState GetInitialState()
    {
        return new(0,  null, 0,ImmutableList<RecentWord>.Empty, 5000);
    }
}

}