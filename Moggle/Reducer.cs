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
}

}