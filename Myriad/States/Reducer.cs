using Fluxor;
using Myriad.Actions;

namespace Myriad.States
{

public static class Reducer
{
    [ReducerMethod] public static Board Reduce(Board board, IAction<Board> action) => action.Reduce(board);
    [ReducerMethod] public static Solver Reduce(Solver solver, IAction<Solver> action) => action.Reduce(solver);
    [ReducerMethod] public static ChosenPositionsState Reduce(ChosenPositionsState solver, IAction<ChosenPositionsState> action) => action.Reduce(solver);
    [ReducerMethod] public static AnimationState Reduce(AnimationState board, IAction<AnimationState> action) => action.Reduce(board);
    [ReducerMethod] public static CheatState Reduce(CheatState board, IAction<CheatState> action) => action.Reduce(board);
    [ReducerMethod] public static FoundWordsState Reduce(FoundWordsState board, IAction<FoundWordsState> action) => action.Reduce(board);
    [ReducerMethod] public static GameSettingsState Reduce(GameSettingsState board, IAction<GameSettingsState> action) => action.Reduce(board);
    [ReducerMethod] public static RecentWordsState Reduce(RecentWordsState board, IAction<RecentWordsState> action) => action.Reduce(board);
    [ReducerMethod] public static TimeState Reduce(TimeState board, IAction<TimeState> action) => action.Reduce(board);
    [ReducerMethod] public static ChallengeGamesHistory Reduce(ChallengeGamesHistory board, IAction<ChallengeGamesHistory> action) => action.Reduce(board);

    [ReducerMethod]
    public static TState Reduce<TState>(TState state, IAction<TState> action) =>
        action.Reduce(state);
}

}
