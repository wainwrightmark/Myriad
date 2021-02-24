using Fluxor;

namespace Moggle
{

public static class Reducer
{
    [ReducerMethod]
    public static MoggleState Reduce(MoggleState board, IAction<MoggleState> action)
    {
        return action.Reduce(board);
    }

    [ReducerMethod]
    public static UIState Reduce(UIState board, IAction<UIState> action)
    {
        return action.Reduce(board);
    }
}

}
