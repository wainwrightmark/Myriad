using Fluxor;

namespace Moggle
{

public class Feature : Feature<MoggleState>
{
    public override string GetName() => "Moggle";
    protected override MoggleState GetInitialState() => MoggleState.DefaultState;
}

}