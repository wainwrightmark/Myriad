using System;
using System.Collections.Generic;
using System.Linq;
using Moggle.States;

namespace Moggle
{

//TODO less data in this action
public record MoveAction(string BoardId, MoveResult Result, Coordinate Coordinate) : IAction<MoggleState>,
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

public record LoadWordsAction(IReadOnlyList<SavedWord> Save) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState state)
    {
        var newWords =
            state.FoundWords.Union(
            Save.Select(x=> state.Solver.CheckLegal(x.wordText))
            .OfType<WordCheckResult.Legal>()
            .Select(x => x.Word));

        var newState  = state with { FoundWords = newWords };

        return newState;
    }
}

}
