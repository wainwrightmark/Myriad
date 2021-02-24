using System.Threading.Tasks;
using Blazored.LocalStorage;
using Fluxor;
using Moggle.States;

namespace Moggle.Blazor.Flux
{

public class LoadWordsEffect : Effect<StartGameAction>
{
    private readonly ILocalStorageService _iLocalStorageService;

    public LoadWordsEffect(ILocalStorageService lss) => _iLocalStorageService = lss;

    /// <inheritdoc />
    public override async Task HandleAsync(StartGameAction action, IDispatcher dispatcher)
    {
        var gameString = GameSettingsState.CreateGameString(action.GameMode, action.Settings);
        var savedGame  = await _iLocalStorageService.GetItemAsync<SavedGame>(gameString);

        if (savedGame != null)
            dispatcher.Dispatch(new LoadWordsAction(savedGame));
    }
}

}
