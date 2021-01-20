using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Fluxor;
using MoreLinq;

namespace Moggle
{

public interface IAction<TState>
{
    TState Reduce(TState board);
}

public record StartGameAction(string Seed, int Duration) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board)
    {
        return board.StartNewGame(Seed, Duration);
    }
}

//public class StartGameEffect : Effect<StartGameAction>
//{
//    /// <inheritdoc />
//    protected override async Task HandleAsync(StartGameAction action, IDispatcher dispatcher)
//    {
//        await Task.Delay(TimeSpan.FromSeconds(action.Duration));
//        dispatcher.Dispatch(new CheckTimeAction());
//    }
//}

//public record CheckTimeAction : IAction<MoggleState>
//{
//    /// <inheritdoc />
//    public MoggleState Reduce(MoggleState board)
//    {
//        if (board.FinishTime.HasValue && board.FinishTime.Value.Ticks < DateTime.Now.Ticks)
//            return board with { FinishTime = null };

//        return board;
//    }
//}

public record RotateAction(bool Clockwise) : IAction<MoggleState>
{
    /// <inheritdoc />
    public MoggleState Reduce(MoggleState board)
    {
        var newRotation = Clockwise ? board.Rotation + 1 : board.Rotation - 1;

        return board with { Rotation = newRotation };
    }
}

public static class Reducer
{
    [ReducerMethod]
    public static MoggleState Reduce(MoggleState board, IAction<MoggleState> action)
    {
        return action.Reduce(board);
    }
}

public record MoggleState(MoggleBoard Board, DateTime FinishTime, int Rotation)
{
    public static readonly MoggleState DefaultState = new(
        MoggleBoard.DefaultBoardClassic,
        DateTime.Now,
        0
    );

    public MoggleState StartNewGame(string seed, int duration)
    {
        var newState = new MoggleState(
            Board.Randomize(seed),
            DateTime.Now.AddSeconds(duration),
            Rotation
        );

        return newState;
    }


    public static (int row, int column) RotateCoordinate(int row, int column, int size, int rotation)
    {
        var realSize = size - 1;
        return ((rotation + 4) % 4) switch
        {
            0 => (row, column),
            1 => (realSize - column, row),
            2 => (realSize - row, realSize - column),
            3 => (column, realSize - row),
            _ => throw new ArgumentException(nameof(Rotation))
        };
    }

    public char? GetLetterAtCoordinate(int row, int column)
    {
        if (Board == null)
            return null;

        var (newRow, newColumn) = RotateCoordinate(row, column, Board.ColumnCount, Rotation);

        return Board.GetLetterAtCoordinate(newRow, newColumn);
    }
}

public class Feature : Feature<MoggleState>
{
    public override string GetName() => "Moggle";
    protected override MoggleState GetInitialState() => MoggleState.DefaultState;
}

public record MoggleBoard(
    ImmutableArray<BoggleDice> Dice,
    ImmutableList<DicePosition> Positions,
    int ColumnCount)
{
    public static readonly MoggleBoard DefaultBoardClassic = new(
        ImmutableArray.Create<BoggleDice>(
            new("AACIOT"),
            new("ABILTY"),
            new("ABJMOQu"),
            new("ACDEMP"),
            new("ACELRS"),
            new("ADENVZ"),
            new("AHMORS"),
            new("BIFORX"),
            new("DENOSW"),
            new("DKNOTU"),
            new("EEFHIY"),
            new("EGKLUY"),
            new("EGINTV"),
            new("EHINPS"),
            new("ELPSTU"),
            new("GILRUW")
        ),
        Enumerable.Range(0, 16).Select(x => new DicePosition(x, 0)).ToImmutableList(),
        4
    );

    public static readonly MoggleBoard DefaultBoardModern = new(
        ImmutableArray.Create<BoggleDice>(
            new("AAEEGN"),
            new("ABBJOO"),
            new("ACHOPS"),
            new("AFFKPS"),
            new("AOOTTW"),
            new("CIMOTU"),
            new("DEILRX"),
            new("DELRVY"),
            new("DISTTY"),
            new("EEGHNW"),
            new("EEINSU"),
            new("EHRTVW"),
            new("EIOSST"),
            new("ELRTTY"),
            new("HIMNUQ"),
            new("HLNNRZ")
        ),
        Enumerable.Range(0, 16).Select(x => new DicePosition(x, 0)).ToImmutableList(),
        4
    );

    public MoggleBoard Randomize(string seed)
    {
        seed = seed.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(seed))
        {
            return this with { Positions = DefaultBoardClassic.Positions };
        }
        else
        {
            return Randomize(new Random(seed.GetHashCode()));
        }
    }

    public MoggleBoard Randomize(Random random)
    {
        var newPositions = Positions.Shuffle(random)
            .Select(x => x.RandomizeFace(random))
            .ToImmutableList();

        return this with { Positions = newPositions };
    }

    public char GetLetterAtCoordinate(int row, int column)
    {
        var index = (row * ColumnCount) + column;

        return GetLetterAtIndex(index);
    }

    public char GetLetterAtIndex(int i)
    {
        var position = Positions[i % Positions.Count];
        var die      = Dice[position.DiceIndex % Dice.Length];
        var letter   = die.Letters[position.FaceIndex % die.Letters.Length];
        return letter;
    }

    public int RowCount => Positions.Count / ColumnCount;
}

public record BoggleDice(string Letters);

public record DicePosition(int DiceIndex, int FaceIndex)
{
    public DicePosition RandomizeFace(Random random)
    {
        var fi = random.Next(5);
        return this with { FaceIndex = fi };
    }
}

}
