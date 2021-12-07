using System.Threading.Tasks;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Myriad.Actions;
using Myriad.States;

namespace Myriad.Blazor.Flux;

public class NavigateEffect : Effect<StartGameAction>
{
    private readonly NavigationManager _navigationManager;

    public NavigateEffect(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <inheritdoc />
    public override async Task HandleAsync(StartGameAction action, IDispatcher dispatcher)
    {
        var gameString = GameSettingsState.CreateGameString(action.GameMode, action.Settings);

        var uri = _navigationManager.BaseUri + $"?{gameString}";
        _navigationManager.NavigateTo(uri);
    }
}