using System;
using System.Linq;
using Moggle.States;

namespace Moggle
{

//TODO less data in this action
public record MoveAction(MoveResult Result, Coordinate Coordinate, string GameString) : IAction<MoggleState>,
    IAction<RecentWordsState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state)
    {
        if (Result is MoveResult.SuccessResult sr)
            return sr.MoggleState;

        return state;
    }

    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        var newState = state with
        {
            RecentWords = state.RecentWords.RemoveAll(x => x.ExpiryDate < DateTime.Now)
        };

        if (Result.AnimationWord is not null)
        {
            var rw = new RecentWord(
                Result.AnimationWord,
                Coordinate,
                state.Rotation,
                DateTime.Now.AddMilliseconds(Result.AnimationWord.LingerDuration)
            );

            newState = newState with { RecentWords = state.RecentWords.Add(rw) };
        }

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
