using System.Collections.Immutable;
using Fluxor;

namespace Moggle.States
{

public class FoundWordsFeature : Feature<FoundWordsState>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "Unchecked Words";
    }

    /// <inheritdoc />
    protected override FoundWordsState GetInitialState()
    {
        return new(ImmutableHashSet<FoundWord>.Empty);
    }
}

}
