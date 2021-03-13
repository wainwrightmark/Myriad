using System.Collections.Immutable;
using Fluxor;

namespace Myriad.States
{

public class ChosenPositionsFeature : Feature<ChosenPositionsState>
{
    /// <inheritdoc />
    public override string GetName() => "Chosen Positions";

    /// <inheritdoc />
    protected override ChosenPositionsState GetInitialState() =>
        new(ImmutableList<Coordinate>.Empty);
}

public class BoardFeature : Feature<Board>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "Myriad Board";
    }

    /// <inheritdoc />
    protected override Board GetInitialState()
    {
        return NumbersGameMode.Instance.CreateBoard(
            ImmutableDictionary<string, string>.Empty,
            WordList.LazyInstance
        );
    }
}

}
