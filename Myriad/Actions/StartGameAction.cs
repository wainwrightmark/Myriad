using System;
using System.Collections.Immutable;
using Myriad.States;

namespace Myriad.Actions;

public record StartGameAction(
    Lazy<WordList> WordList,
    IGameMode GameMode,
    ImmutableDictionary<string, string> Settings) :
    IAction<Board>,
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
    public Board Reduce(Board state)
    {
        return GameMode.CreateBoard(Settings, WordList);
    }

    /// <inheritdoc />
    public AnimationState Reduce(AnimationState state)
    {
        var animation = GameMode.GetAnimation(Settings, WordList);

        var newState = new AnimationState(animation, animation == null? null : DateTime.Now, state.FrameMs, 0);
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
        return new(ImmutableList<RecentWord>.Empty, new Random().Next(4), new Random().Next(2) == 0);
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