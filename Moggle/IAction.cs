namespace Moggle
{

public interface IAction<TState>
{
    TState Reduce(TState state);
}

}
