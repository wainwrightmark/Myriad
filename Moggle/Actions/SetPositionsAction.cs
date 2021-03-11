using System.Collections.Immutable;
using Moggle.States;

namespace Moggle.Actions
{

public record SetPositionsAction
    (ImmutableList<Coordinate> NewCoordinates) : IAction<ChosenPositionsState>
{
    /// <inheritdoc />
    public ChosenPositionsState Reduce(ChosenPositionsState state)
    {
        return new(NewCoordinates);
    }
}

}
