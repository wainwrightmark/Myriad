using System.Threading;
using System.Threading.Tasks;
using Fluxor;

namespace Moggle
{

public class CheatEffect : Effect<CheatAction>
{
    /// <inheritdoc />
    protected override async Task HandleAsync(CheatAction action, IDispatcher dispatcher)
    {
        if (_solver == null)
        {
            var s = await Solver.FromDictionaryHelperAsync(CancellationToken.None);

            _solver = s;
        }

        dispatcher.Dispatch(new SolveAction(_solver));
    }

    private static Solver? _solver;
}

}