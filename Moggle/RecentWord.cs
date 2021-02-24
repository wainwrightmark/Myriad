using System;

namespace Moggle
{

public record RecentWord(
    AnimationWord Word,
    Coordinate Coordinate,
    int Rotate,
    DateTime ExpiryDate);

}