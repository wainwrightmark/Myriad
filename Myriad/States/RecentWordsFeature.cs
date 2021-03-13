using System.Collections.Immutable;
using Fluxor;

namespace Myriad.States
{

public class RecentWordsFeature : Feature<RecentWordsState>
{
    /// <inheritdoc />
    public override string GetName() => "RecentWords";

    /// <inheritdoc />
    protected override RecentWordsState GetInitialState()
    {
        return new(ImmutableList<RecentWord>.Empty, 0, false);
    }
}

}
