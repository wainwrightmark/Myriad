using System.Collections.Immutable;

namespace Moggle.States
{

public record CheatState(
    bool AllowCheating,
    bool Revealed,
    ImmutableList<FoundWord> PossibleWords) { }

}