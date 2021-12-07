using System.Collections.Immutable;
using Fluxor;

namespace Myriad.States;

public class ChallengeGamesHistoryFeature : Feature<ChallengeGamesHistory>
{
    /// <inheritdoc />
    public override string GetName() => nameof(ChallengeGamesHistory);

    /// <inheritdoc />
    protected override ChallengeGamesHistory GetInitialState()
    {
        return new(ImmutableDictionary<string, SavedChallengeGame>.Empty);
    }
}