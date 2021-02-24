using System;
using System.Collections.Immutable;
using Fluxor;

namespace Moggle
{

public class MoggleStateFeature : Feature<MoggleState>
{
    public override string GetName() => "Moggle";
    protected override MoggleState GetInitialState() => MoggleState.DefaultState;
}

public class UIStateFeature : Feature<UIState>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "UIState";
    }

    /// <inheritdoc />
    protected override UIState GetInitialState()
    {
        return new(0,  null, 0,ImmutableList<RecentWord>.Empty, 5000);
    }
}

public record UIState(
    int Rotate,
    Animation? Animation,
    int AnimationFrame,
    ImmutableList<RecentWord> RecentWords,
    int LingerDuration)
{

}

public record RecentWord(
    AnimationWord Word,
    Coordinate Coordinate,
    int Rotate,
    DateTime ExpiryDate);

}

