namespace Moggle
{

public record RotateAction(int Amount) : IAction<UIState>
{

    /// <inheritdoc />
    public UIState Reduce(UIState state)
    {
        return state with { Rotate = state.Rotate + Amount };
    }
}


}
