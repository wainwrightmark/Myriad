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
        return new(0,  null);
    }
}

public record UIState(int Rotate, Animation? Animation) { }

}
