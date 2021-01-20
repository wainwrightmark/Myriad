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
    ImmutableHashSet<string> FoundWords,
    ImmutableHashSet<string> DisabledWords)
{
    public static readonly MoggleState DefaultState = new(
        MoggleBoard.DefaultBoardClassic,
        null,
        0,
        ImmutableList<Coordinate>.Empty,
        ImmutableHashSet<string>.Empty,
        ImmutableHashSet<string>.Empty
    );

    public MoggleState StartNewGame(string seed, int duration)
    {
        var newState = new MoggleState(
            Board.Randomize(seed),
            DateTime.Now.AddSeconds(duration),
            Rotation,
            ImmutableList<Coordinate>.Empty,
            ImmutableHashSet<string>.Empty,
            ImmutableHashSet<string>.Empty
        );

        return newState;
    }

    public bool IsMoveLegal(Coordinate coordinate) => TryGetMoveResult(coordinate) != null;

    public MoggleState? TryGetMoveResult(Coordinate coordinate)
    {
        if (FinishTime == null)
            return null;

        if (!ChosenPositions.Any())
            return this with { ChosenPositions = ChosenPositions.Add(coordinate) };

        if (ChosenPositions.Last().Equals(coordinate))
        {
            if (ChosenPositions.Count >= 3) //Complete a word
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
            else if (ChosenPositions.Count == 1) //Give up on this path
                return this with { ChosenPositions = ImmutableList<Coordinate>.Empty };
            else
                return null; //Do nothing
        }

        var index = ChosenPositions.LastIndexOf(coordinate);

        if (index >= 0) //Go back some number of steps
            return this with
            {
                ChosenPositions = ChosenPositions.Take(index + 1).ToImmutableList()
            };

        if (ChosenPositions.Last().IsAdjacent(coordinate)) //add this coordinate to the list
            return this with { ChosenPositions = ChosenPositions.Add(coordinate) };

        return null;
    }

    public char GetLetterAtCoordinate(Coordinate coordinate)
    {
        var newCoordinate = coordinate.Rotate(Board.ColumnCount, Rotation);
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
                    case <3:  break;
                    case 3:   total += 1;break;
                    case 4:   total += 1;break;
                    case 5:   total += 2;break;
                    case 6:   total += 3;break;
                    case 7:   total += 4;break;
                    case >=8: total += 11;break;
                }
            }

            return total;
        }
    }
}

}
