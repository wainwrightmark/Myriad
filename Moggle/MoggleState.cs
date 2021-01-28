using System;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public record EnableWord(string Word, bool Enable) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board)
    {
        if (Enable)
            return board with { DisabledWords = board.DisabledWords.Remove(Word) };

        return board with { DisabledWords = board.DisabledWords.Add(Word) };
    }
}

public record MoggleState(
    MoggleBoard Board,
    DateTime? FinishTime,
    int Rotation,
    ImmutableList<Coordinate> ChosenPositions,
    ImmutableSortedSet<string> FoundWords,
    ImmutableHashSet<string> DisabledWords)
{
    public static readonly MoggleState DefaultState = new(
        MoggleBoard.Create(true, 4, 4),
        null,
        0,
        ImmutableList<Coordinate>.Empty,
        ImmutableSortedSet<string>.Empty,
        ImmutableHashSet<string>.Empty
    );

    public MoggleState StartNewGame(
        string seed,
        int width,
        int height,
        bool classic,
        int duration)
    {
        var newState = new MoggleState(
            MoggleBoard.Create(classic, width, height).Randomize(seed),
            DateTime.Now.AddSeconds(duration),
            Rotation,
            ImmutableList<Coordinate>.Empty,
            ImmutableSortedSet<string>.Empty,
            ImmutableHashSet<string>.Empty
        );

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
            switch (ChosenPositions.Count)
            {
                //Complete a word
                case >= 3:
                {
                    var word =
                        new string(
                            ChosenPositions.Select(GetLetterAtCoordinate)
                                .ToArray()
                        );

                    return this with
                    {
                        ChosenPositions = ImmutableList<Coordinate>.Empty,
                        FoundWords = FoundWords.Add(word)
                    };
                }
                //Give up on this path
                case 1 or 2:  return this with { ChosenPositions = ImmutableList<Coordinate>.Empty };
                default: return null; //Do nothing
            }
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

    public char GetLetterAtCoordinate(Coordinate coordinate)
    {
        var newCoordinate = coordinate.Rotate(Board.MaxCoordinate, Rotation);
        return Board.GetLetterAtCoordinate(newCoordinate);
    }

    public int Score
    {
        get
        {
            var total = 0;

            foreach (var s in FoundWords.Except(DisabledWords))
            {
                switch (s.Length)
                {
                    case <3: break;
                    case 3:
                        total += 1;
                        break;
                    case 4:
                        total += 1;
                        break;
                    case 5:
                        total += 2;
                        break;
                    case 6:
                        total += 3;
                        break;
                    case 7:
                        total += 4;
                        break;
                    case >=8:
                        total += 11;
                        break;
                }
            }

            return total;
        }
    }
}

}
