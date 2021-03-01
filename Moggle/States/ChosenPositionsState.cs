using System.Collections.Immutable;

namespace Moggle.States
{

public record ChosenPositionsState(ImmutableList<Coordinate> ChosenPositions) { }

}