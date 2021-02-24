using System.Threading.Tasks;
using Blazored.LocalStorage;
using Fluxor;

namespace Moggle.Blazor.Flux
{

public class SaveWordEffect : Effect<MoveAction>
{
    private readonly ILocalStorageService _iLocalStorageService;

    public SaveWordEffect(ILocalStorageService lss) => _iLocalStorageService = lss;

    /// <inheritdoc />
    public override async Task HandleAsync(MoveAction action, IDispatcher dispatcher)
    {
        if (action.Result is MoveResult.WordComplete wc)
        {
            var sessionInfo = SavedGame.Create(wc.MoggleState);
            await _iLocalStorageService.SetItemAsync(sessionInfo.GameString, sessionInfo);
        }
    }
}

}
