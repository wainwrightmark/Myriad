using System;
using System.Threading;
using System.Threading.Tasks;
using Fluxor;

namespace Moggle
{

public class StartGameEffect : Effect<StartGameAction>
{
    /// <inheritdoc />
    protected override async Task HandleAsync(StartGameAction action, IDispatcher dispatcher)
    {
        Timer t = null;

        t = new Timer(
            _ =>
            {
                dispatcher.Dispatch(new CheckTimeAction());
                t.DisposeAsync();
            },
            null,
            TimeSpan.FromSeconds(action.Duration),
            Timeout.InfiniteTimeSpan
        );
    }
}

}