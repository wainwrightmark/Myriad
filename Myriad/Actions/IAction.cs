namespace Myriad.Actions
{

public interface IAction<TState>
{
    TState Reduce(TState state);
}

}
