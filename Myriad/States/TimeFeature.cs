using Fluxor;

namespace Myriad.States
{

public class TimeFeature : Feature<TimeState>
{
    /// <inheritdoc />
    public override string GetName() => "Time";

    /// <inheritdoc />
    protected override TimeState GetInitialState() => new(TimeSituation.Infinite.Instance);
}

}
