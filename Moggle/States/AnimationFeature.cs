﻿using Fluxor;

namespace Moggle.States
{

public class AnimationFeature : Feature<AnimationState>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "Animation";
    }

    /// <inheritdoc />
    protected override AnimationState GetInitialState()
    {
        return new(null, 0);
    }
}

}