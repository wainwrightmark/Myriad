using System.Collections.Immutable;

namespace Moggle
{

public record StartGameAction(
    WordList WordList,
    IMoggleGameMode GameMode,
    ImmutableDictionary<string, string> Settings,
    int Duration) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board) => MoggleState.StartNewGame(
        WordList,
        GameMode,
        Settings,
        Duration
    );
}

}
