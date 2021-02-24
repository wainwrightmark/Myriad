using System.Collections.Immutable;

namespace Moggle
{

public record UIState(
    int Rotate,
    Animation? Animation,
    int AnimationFrame,
    ImmutableList<RecentWord> RecentWords,
    int LingerDuration)
{

}

}