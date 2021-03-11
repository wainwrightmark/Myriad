using System;

namespace Moggle.States
{

public record AnimationState(
    Animation? Animation,
    DateTime? LastFrame,
    int FrameMs, //The number of ms per frame
    int AnimationFrame) { }

}
