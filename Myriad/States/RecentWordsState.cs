using System.Collections.Immutable;

namespace Myriad.States;

public record RecentWordsState(ImmutableList<RecentWord> RecentWords, int Rotation, bool Flip) { }