using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Myriad.States;

namespace Myriad
{

public abstract record Step
{
    public record Rotate(int Amount) : Step;
    //public record Move(Coordinate Coordinate) : Step;[]
    public record SetFoundWord(FoundWord Word) : Step;
    public record ClearPositionsStep : Step;
}

public record StepWithResult(Step Step, MoveResult? MoveResult, int NewIndex) { }

public record Animation(ImmutableList<Step> Steps)
{
    public StepWithResult GetStepWithResult(
        ChosenPositionsState cps,
        Board mb,
        Solver solver,
        FoundWordsState fws,
        int index)
    {
        var c = Steps[index % Steps.Count];

        switch (c)
        {
            case Step.ClearPositionsStep clearCoordinatesAction:
            {
                return new StepWithResult(
                    clearCoordinatesAction,
                    new MoveResult.WordAbandoned(),
                    index
                );
            }
            //case Step.Move move:
            //{
            //    var mr = MoveResult.GetMoveResult(move.Coordinate, cps, mb, solver, fws);
            //    return new StepWithResult(c, mr, index);
            //}
            case Step.SetFoundWord findWord:
            {
                return new StepWithResult(
                    findWord,
                    new MoveResult.WordComplete(
                        findWord.Word,
                        findWord.Word.Path
                    ),
                    index
                );
            }

            case Step.Rotate: return new StepWithResult(c, null, index);
            default:          throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    public static Animation? Create(IEnumerable<string> allWords, Board board)
    {
        var steps = new List<Step>();

        foreach (var word in allWords)
        {
            var path = board.TryFindWord(word);
            if (path is not null && path.Any())
            {
                var fw = FoundWord.Create(word, path);
                steps.Add(new Step.SetFoundWord(fw));

                //steps.AddRange(cs.Select(x => new Step.Move(x)));
                //steps.Add(new Step.Rotate(1));
                //steps.Add(new Step.Move(cs.Last()));
            }
        }

        return steps.Any() ? new Animation(steps.ToImmutableList()) : null;
    }

    public static Animation? Create(IEnumerable<FoundWord> allWords)
    {
        var steps = new List<Step>();

        foreach (var word in allWords)
        {
            steps.Add(new Step.SetFoundWord(word));
        }

        return steps.Any() ? new Animation(steps.ToImmutableList()) : null;
    }
}

}
