using System;
using System.Collections.Immutable;

namespace Moggle
{

public record StartGameAction(
    Lazy<WordList> WordList,
    IMoggleGameMode GameMode,
    ImmutableDictionary<string, string> Settings,
    SavedGame? SavedGame
    ) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board) => MoggleState.StartNewGame(
        WordList,
        GameMode,
        Settings,
        SavedGame
    );
}

}
