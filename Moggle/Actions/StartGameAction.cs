using System;
using System.Collections.Immutable;
using Moggle.States;

namespace Moggle.Actions
{

public record StartGameAction(
    Lazy<WordList> WordList,
    IMoggleGameMode GameMode,
    ImmutableDictionary<string, string> Settings) :
    IAction<MoggleBoard>,
    IAction<Solver>,
    IAction<ChosenPositionsState>,
    IAction<CheatState>,
    IAction<AnimationState>,
    IAction<GameSettingsState>,
    IAction<RecentWordsState>,
    IAction<TimeState>,
    IAction<FoundWordsState>
{
    /// <inheritdoc />
    public ChosenPositionsState Reduce(ChosenPositionsState state)
    {
        return new(ImmutableList<Coordinate>.Empty);
    }

    /// <inheritdoc />
    public Solver Reduce(Solver state)
    {
        return GameMode.CreateSolver(Settings, WordList);
    }

    /// <inheritdoc />
    public MoggleBoard Reduce(MoggleBoard state)
    {
        return GameMode.CreateBoard(Settings, WordList);
    }

    /// <inheritdoc />
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

        return state;
    }

    /// <inheritdoc />
    public GameSettingsState Reduce(GameSettingsState settingsState)
    {
        return new(GameMode, Settings);
    }

    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        return new(ImmutableList<RecentWord>.Empty, 0, false);
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
        var data = GameMode.GetFoundWordsData(Settings, WordList);

        return new FoundWordsState(data);
    }
}

}
