using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moggle.States;

namespace Moggle
{

public abstract record Step
{
    public record Rotate(int Amount) : Step;
    public record Move(Coordinate Coordinate) : Step;
    public record SetCoordinatesAction(ImmutableList<Coordinate> Path) : Step;
}

public record StepWithResult(Step Step, MoveResult? MoveResult, int NewIndex) { }

public record Animation(ImmutableList<Step> Steps)
{
    public StepWithResult GetStepWithResult(ChosenPositionsState cps, MoggleBoard mb, Solver solver, FoundWordsState fws, int index)
    {
        var c = Steps[index % Steps.Count];

        switch (c)
        {
            case Step.Move move:
            {
                var mr = MoveResult.GetMoveResult(move.Coordinate, cps, mb, solver, fws);
                return new StepWithResult(c, mr, index + 1);
            }
            case Step.Rotate: return new StepWithResult(c, null, index + 1);
            default:          throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

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

    public static Animation? CreateForAllSolutions(MoggleBoard board, Solver solver, bool reverseAnimationOrder)
    {

        var paths =
            RemoveRedundant(
                solver.GetPossibleSolutions(board),
                solver,
                board
            );

        var sortedPaths =
            reverseAnimationOrder
                ? paths.OrderByDescending(x => x, ListComparer<Coordinate>.Instance)
                : paths.OrderBy(x => x, ListComparer<Coordinate>.Instance);

        var steps = new List<Step>();

        var previous = ImmutableList<Coordinate>.Empty;

        foreach (var path in sortedPaths)
        {
            var stepsInCommon =
                path.Zip(previous)
                    .TakeWhile(x => x.First.Equals(x.Second))
                    .Select(x => x.First)
                    .ToList();

            if (!stepsInCommon.Any())
            {
                if (previous.Any())
                    steps.Add(new Step.SetCoordinatesAction(ImmutableList<Coordinate>.Empty)); //abandon

                steps.AddRange(path.Select(x => new Step.Move(x)));
            }
            else if (stepsInCommon.Count == previous.Count) //continue
            {
                steps.AddRange(path.Skip(stepsInCommon.Count).Select(x => new Step.Move(x)));
            }
            else //backtrack then continue
            {
                steps.Add(new Step.SetCoordinatesAction(ImmutableList<Coordinate>.Empty));
                steps.AddRange(path.Skip(stepsInCommon.Count).Select(x => new Step.Move(x)));
            }

            previous = path;
        }

        if (!steps.Any())
            return null;

        steps.Add(steps.Last());
        steps.Add(new Step.Rotate(1));

        return new Animation(steps.ToImmutableList());

        static IEnumerable<ImmutableList<Coordinate>> RemoveRedundant(
            IEnumerable<FoundWord> initial,
            Solver solver,
            MoggleBoard board)
        {
            var everything = initial.Select(
                    foundWord =>
                        (foundWord,
                         subvalues: GetAllSubValues(foundWord).ToList())
                )
                .ToDictionary(x => x.foundWord);

            var cont = true;

            while (cont)
            {
                cont = false;

                var singleSourceWords = everything.Values
                    .SelectMany(x => x.subvalues.Append(x.foundWord))
                    .GroupBy(x => x)
                    .Where(x => x.Count() == 1)
                    .Select(x => x.Key)
                    .ToList();

                foreach (var singleSourceWord in singleSourceWords)
                {
                    if (everything.Remove(singleSourceWord, out var av))
                    {
                        cont = true; //we have successfully removed something
                        yield return av.foundWord.Path;

                        foreach (var subWord in av.subvalues)
                            everything.Remove(subWord);
                    }
                }
            }

            while (everything.Any()) //Hopefully there will not be much left
            {
                var e = everything.First();

                yield return e.Value.foundWord.Path;

                everything.Remove(e.Key);

                foreach (var valueSubvalue in e.Value.subvalues)
                {
                    everything.Remove(valueSubvalue);
                }
            }

            IEnumerable<FoundWord> GetAllSubValues(FoundWord word)
            {
                for (var i = 1; i < word.Path.Count; i++)
                {
                    var subPath = word.Path.GetRange(0, i);

                    var text = word.Text.Substring(0, i);

                    var r = solver.CheckLegal(text, subPath);

                    if (r is WordCheckResult.Legal legal)
                        yield return legal.Word;
                }
            }
        }
    }
}

}
