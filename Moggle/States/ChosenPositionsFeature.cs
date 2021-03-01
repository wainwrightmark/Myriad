using System.Collections.Immutable;
using Fluxor;

namespace Moggle.States
{

public class ChosenPositionsFeature : Feature<ChosenPositionsState>
{
    /// <inheritdoc />
    public override string GetName() => "Chosen Positions";

    /// <inheritdoc />
    protected override ChosenPositionsState GetInitialState() =>
        new(ImmutableList<Coordinate>.Empty);
}

public class MoggleBoardFeature : Feature<MoggleBoard>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "Moggle Board";
    }

    /// <inheritdoc />
    protected override MoggleBoard GetInitialState()
    {
        return CenturyGameMode.Instance.CreateGame(
                ImmutableDictionary<string, string>.Empty,
                WordList.LazyInstance
            )
            .board;
    }
}

}
