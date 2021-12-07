using System;
using System.Collections.Immutable;
using System.Linq;
using Myriad.States;

namespace Myriad.Actions;

public record SetPositionsAction
    (ImmutableList<Coordinate> NewCoordinates, AnimationWord? AnimationWord) : IAction<ChosenPositionsState>, IAction<RecentWordsState>
{
    /// <inheritdoc />
    public ChosenPositionsState Reduce(ChosenPositionsState state)
    {
        return new(NewCoordinates);
    }

    /// <inheritdoc />
    public RecentWordsState Reduce(RecentWordsState state)
    {
        var newState = state with
        {
            RecentWords = state.RecentWords.RemoveAll(x => x.ExpiryDate < DateTime.Now)
        };

        if (AnimationWord is not null && NewCoordinates.Any())
        {
            var rw = new RecentWord(
                AnimationWord,
                NewCoordinates.Last(),
                state.Rotation,
                state.Flip,
                DateTime.Now.AddMilliseconds(AnimationWord.LingerDuration)
            );

            newState = newState with { RecentWords = state.RecentWords.Add(rw) };
        }

        return newState;
    }
}