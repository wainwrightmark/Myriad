using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Moggle.Creator;

namespace Moggle
{

public record MoggleState(
    MoggleBoard Board,
    DateTime? FinishTime,
    int Rotation,
    ImmutableList<Coordinate> ChosenPositions,
    ImmutableSortedSet<string> FoundWords,
    ImmutableHashSet<string> DisabledWords,
    ImmutableList<string>? CheatWords)
{

    public static readonly Solver Solver = Solver.FromResourceFile();


    public static readonly MoggleState DefaultState = new(
        MoggleBoard.Create(false, 4, 4, 3),
        null,
        0,
        ImmutableList<Coordinate>.Empty,
        ImmutableSortedSet<string>.Empty,
        ImmutableHashSet<string>.Empty,
        null
    );

    public static MoggleState CreateFromString(string s) => new(
        MoggleBoard.CreateFromString(s),
        null,
        0,
        ImmutableList<Coordinate>.Empty,
        ImmutableSortedSet<string>.Empty,
        ImmutableHashSet<string>.Empty,
        null
    );

    public MoggleState StartNewGame(
        string seed,
        int width,
        int height,
        bool classic,
        int duration,
        int minWordLength)
    {
        MoggleState newState;

        if (seed.StartsWith('_')) //this is a bit hacky
        {
            newState = CreateFromString(seed.TrimStart('_'))
                with
                {
                    FinishTime = DateTime.Now.AddSeconds(duration)
                };
        }
        else if (seed.StartsWith("?")) //also cheeky
        {
            var grid = GridCreator.CreateNodeGridFromText(
                seed.TrimStart('?'),
                NullLogger<MoggleState>.Instance,
                1000
            );

            newState = new MoggleState(
                grid.ToMoggleBoard(MoggleBoard.PaddingRune),
                DateTime.Now.AddSeconds(duration),
                Rotation,
                ImmutableList<Coordinate>.Empty,
                ImmutableSortedSet<string>.Empty,
                ImmutableHashSet<string>.Empty,
                null
            );
        }
        else
        {
            newState = new MoggleState(
                MoggleBoard.Create(classic, width, height, minWordLength).Randomize(seed),
                DateTime.Now.AddSeconds(duration),
                Rotation,
                ImmutableList<Coordinate>.Empty,
                ImmutableSortedSet<string>.Empty,
                ImmutableHashSet<string>.Empty,
                null
            );
        }

        return newState;
    }

    public int MaxDimension => Math.Max(Board.Width, Board.Height);

    public bool IsMoveLegal(Coordinate coordinate) => TryGetMoveResult(coordinate) != null;

    public MoggleState? TryGetMoveResult(Coordinate coordinate)
    {
        if (FinishTime == null)
            return null;

        if (!ChosenPositions.Any())
            return this with { ChosenPositions = ChosenPositions.Add(coordinate) };

        if (ChosenPositions.Last().Equals(coordinate))
        {

            if (ChosenPositions.Count >= Board.MinWordLength)//Complete a word
            {
                var word = string.Join(
                    "",
                    ChosenPositions.Select(GetLetterAtCoordinate).Select(x => x.WordText)
                );

                if (!Solver.LegalWords.Contains(word))
                    return null;

                return this with
                {
                    ChosenPositions = ImmutableList<Coordinate>.Empty,
                    FoundWords = FoundWords.Add(word)
                };
            }
            else if (ChosenPositions.Count <= Board.MinWordLength) //Give up on this path
            {
                return this with { ChosenPositions = ImmutableList<Coordinate>.Empty };
            }
            else//Do nothing
                return null;
        }

        var index = ChosenPositions.LastIndexOf(coordinate);

        switch (index)
        {
            //Give up on this path
            case 0 or 1: return this with { ChosenPositions = ImmutableList<Coordinate>.Empty };
            //Go back some number of steps
            case > 1:
                return this with
                {
                    ChosenPositions = ChosenPositions.Take(index + 1).ToImmutableList()
                };
        }

        if (ChosenPositions.Last().IsAdjacent(coordinate)) //add this coordinate to the list
            return this with { ChosenPositions = ChosenPositions.Add(coordinate) };

        return null;
    }

    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var newCoordinate = coordinate.Rotate(Board.MaxCoordinate, Rotation);
        return Board.GetLetterAtCoordinate(newCoordinate);
    }

    public static int ScoreWord(int length)
    {
        return length switch
        {
            < 3  => 0,
            3    => 1,
            4    => 1,
            5    => 2,
            6    => 3,
            7    => 4,
            >= 8 => 11
        };
    }

    public int Score
    {
        get
        {
            return Words.Sum(s => ScoreWord(s.Length));
        }
    }

    public int NumberOfWords => Words.Count();

    private IEnumerable<string> Words
    {
        get
        {
            if (CheatWords != null)
                return CheatWords;

            return FoundWords.Except(DisabledWords);
        }
    }
}

}
