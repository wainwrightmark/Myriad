using System;
using System.Linq;

namespace Moggle
{

public record MoveAction(MoveResult Result, Coordinate Coordinate) : IAction<MoggleState>,
    IAction<UIState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state)
    {
        if (Result is MoveResult.SuccessResult sr)
            return sr.MoggleState;

        return state;
    }

    /// <inheritdoc />
    public UIState Reduce(UIState state)
    {
        var newState = state;
        if (Result.AnimationWord is not null)
        {
            var rw = new RecentWord(
                Result.AnimationWord,
                Coordinate,
                state.Rotate,
                DateTime.Now.AddMilliseconds(state.LingerDuration)
            );

            newState = newState with { RecentWords = state.RecentWords.Add(rw) };
        }

        newState = newState with
        {
            RecentWords = newState.RecentWords.RemoveAll(x => x.ExpiryDate < DateTime.Now)
        };

        return newState;
    }
}

public record LoadWordsAction(SavedGame Save) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state)
    {
        var newWords = Save.FoundWords.Select(state.Solver.CheckLegal)
            .OfType<WordCheckResult.Legal>()
            .Select(x => x.Word);

        state = state with { FoundWords = state.FoundWords.Union(newWords) };

        return state;
    }
}


}
