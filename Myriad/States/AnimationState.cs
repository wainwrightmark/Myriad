using System;

namespace Myriad.States
{

public record AnimationState(
    Animation? Animation,
    DateTime? LastFrame,
    int FrameMs, //The number of ms per frame
    int AnimationFrame) { }

}
