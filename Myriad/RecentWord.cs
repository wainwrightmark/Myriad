using System;

namespace Myriad;

public record RecentWord(
    AnimationWord Word,
    Coordinate Coordinate,
    int Rotate,
    bool Flip,
    DateTime ExpiryDate);