using System.Collections.Immutable;

namespace Moggle.States
{

public record RecentWordsState(ImmutableList<RecentWord> RecentWords, int Rotation, bool Flip) { }

}
