using Fluxor;

namespace Moggle
{

public class MoggleStateFeature : Feature<MoggleState>
{
    public override string GetName() => "Moggle";
    protected override MoggleState GetInitialState() => MoggleState.DefaultState;
}

}

