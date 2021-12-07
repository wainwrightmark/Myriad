using System;
using Myriad.States;

namespace Myriad.Actions;

public record MoveAction(string BoardId, MoveResult Result, Coordinate Coordinate)
    : IAction<ChosenPositionsState>,
      IAction<RecentWordsState>,
      IAction<FoundWordsState>
{
    /// <inheritdoc />
    public ChosenPositionsState Reduce(ChosenPositionsState state)
    {
        if (Result is MoveResult.SuccessResult sr)
            return state with { ChosenPositions = sr.NewCoordinates };

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
                state.Flip,
                DateTime.Now.AddMilliseconds(Result.AnimationWord.LingerDuration)
            );

            newState = newState with { RecentWords = state.RecentWords.Add(rw) };
        }

        return newState;
    }

    public FoundWordsState Reduce(FoundWordsState state)
    {
        if (Result is MoveResult.WordComplete wc)
            return state.FindWord(wc.FoundWord);

        return state;
    }
}