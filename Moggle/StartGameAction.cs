using System;
using System.Collections.Immutable;
using Moggle.States;

namespace Moggle
{

public record StartGameAction(
    Lazy<WordList> WordList,
    IMoggleGameMode GameMode,
    ImmutableDictionary<string, string> Settings) :
    IAction<MoggleState>,
    IAction<CheatState>,
    IAction<AnimationState>,
    IAction<GameSettingsState>,
    IAction<RecentWordsState>,
    IAction<TimeState>,
    IAction<FoundWordsState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board) => MoggleState.StartNewGame(
        WordList,
        GameMode,
        Settings
    );

    public AnimationState Reduce(AnimationState state)
    {
        var animation = GameMode.GetAnimation(Settings, WordList);

        var newState = new AnimationState(animation, 0);
        return newState;
    }

    /// <inheritdoc />
    public CheatState Reduce(CheatState state)
    {
        if (Settings.ContainsKey("cheat"))
            state = state with { AllowCheating = true };

        ImmutableList<FoundWord> possibleWords;

        if (state.AllowCheating)
        {
            var g = GameMode.CreateGame(Settings, WordList);

            possibleWords = g.Solver.GetPossibleSolutions(g.board).ToImmutableList();
        }
        else
            possibleWords = ImmutableList<FoundWord>.Empty;

        return new CheatState(state.AllowCheating, false, possibleWords);
    }

    /// <inheritdoc />
    public GameSettingsState Reduce(GameSettingsState settingsState)
    {
        return new(GameMode, Settings);
    }

    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        return new(state.RecentWords, state.Rotation);
    }

    /// <inheritdoc />
    public TimeState Reduce(TimeState state)
    {
        var ts = GameMode.CreateTimeSituation(Settings);

        return new TimeState(ts);
    }

    /// <inheritdoc />
    public FoundWordsState Reduce(FoundWordsState state)
    {
        return new (ImmutableHashSet<FoundWord>.Empty);
    }
}

}
