using System;
using System.Collections.Immutable;

namespace Moggle
{

public record StartGameAction(
    Lazy<WordList> WordList,
    IMoggleGameMode GameMode,
    ImmutableDictionary<string, string> Settings,
    SavedGame? SavedGame) : IAction<MoggleState>, IAction<UIState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board) => MoggleState.StartNewGame(
        WordList,
        GameMode,
        Settings,
        SavedGame
    );

    public UIState Reduce(UIState state)
    {
        var animation = GameMode.GetAnimation(Settings, WordList);

        var newState = state with { Animation = animation, Rotate = state.Rotate + 1};
        return newState;
    }
}

}
