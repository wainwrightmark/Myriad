using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public abstract record Step
{
    public record Rotate(int Amount) : Step;
    public record Move(Coordinate Coordinate) : Step;
}

public record StepWithResult(Step Step, MoveResult? MoveResult, int NewIndex)
{

}

public record Animation(ImmutableList<Step> Steps)
{
    public StepWithResult GetStepWithResult(MoggleState state, int index)
    {
        var c = Steps[index % Steps.Count];

        switch (c)
        {
            case Step.Move move:
            {
                var mr = state.TryGetMoveResult(move.Coordinate);
                return new StepWithResult(c, mr, index + 1);
            }
            case Step.Rotate: return new StepWithResult(c, null, index +1 );
            default:                 throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    //public Step Current => Steps[CurrentIndex % Steps.Count];

    //public (Animation animation, Step step) Increment() => (this with { CurrentIndex = CurrentIndex + 1 }, Current);

    public static Animation? Create(IEnumerable<string> allWords, MoggleBoard board)
    {
        var steps = new List<Step>();

        foreach (var word in allWords)
        {
            var cs = board.TryFindWord(word);

            if (cs is not null && cs.Any())
            {
                steps.AddRange(cs.Select(x => new Step.Move(x)));
                steps.Add(new Step.Rotate(1));
                steps.Add(new Step.Move(cs.Last()));
            }
        }

        return steps.Any() ? new Animation(steps.ToImmutableList()) : null;
    }
}

}
