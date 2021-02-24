using System;
using System.Collections.Immutable;

namespace Moggle
{

public record StartGameAction(
    Lazy<WordList> WordList,
    IMoggleGameMode GameMode,
    ImmutableDictionary<string, string> Settings) : IAction<MoggleState>, IAction<UIState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board) => MoggleState.StartNewGame(
        WordList,
        GameMode,
        Settings
    );

    public UIState Reduce(UIState state)
    {
        var animation = GameMode.GetAnimation(Settings, WordList);

        var newState = state with { Animation = animation, Rotate = (state.Rotate + 1) % 4};
        return newState;
    }
}

}
