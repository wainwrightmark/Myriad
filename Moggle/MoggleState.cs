using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public abstract record TimeSituation
{
    public record Infinite : TimeSituation
    {
        private Infinite() { }
        public static Infinite Instance { get; } = new();

        /// <inheritdoc />
        public override bool IsFinished => false;
    }

    public record Finished : TimeSituation
    {
        private Finished() { }
        public static Finished Instance { get; } = new();

        /// <inheritdoc />
        public override bool IsFinished => true;
    }

    public record FinishAt(DateTime DateTime) : TimeSituation
    {
        /// <inheritdoc />
        public override bool IsFinished => DateTime.CompareTo(DateTime.Now) < 0;
    }

    public abstract bool IsFinished { get; }

    public static TimeSituation Create(int duration)
    {
        TimeSituation ts = duration <= 0
            ? Infinite.Instance
            : new FinishAt(DateTime.Now.AddSeconds(duration));

        return ts;
    }

    public static readonly Setting.Integer Duration = new(
        nameof(Duration),
        -1,
        int.MaxValue,
        120,
        10
    );
}

public record MoggleState(
    MoggleBoard Board,
    Solver Solver,
    TimeSituation TimeSituation,
    int Rotation,
    ImmutableList<Coordinate> ChosenPositions,
    ImmutableSortedSet<FoundWord> FoundWords,
    ImmutableHashSet<FoundWord> DisabledWords,
    ImmutableList<FoundWord>? CheatWords,
    IMoggleGameMode LastGameMode,
    ImmutableDictionary<string, string> LastSettings)
{
    public static readonly MoggleState DefaultState =
        StartNewGame(
            WordList.LazyInstance,
            CenturyGameMode.Instance,
            ImmutableDictionary<string, string>.Empty, null
        );

    public static MoggleState StartNewGame(
        WordList wordList,
        IMoggleGameMode gameMode,
        ImmutableDictionary<string, string> settings,
        SavedGame? savedGame) => StartNewGame(
        new Lazy<WordList>(() => wordList),
        gameMode,
        settings,
        savedGame
    );

    public static MoggleState StartNewGame(
        Lazy<WordList> wordList,
        IMoggleGameMode gameMode,
        ImmutableDictionary<string, string> settings, SavedGame? savedGame)
    {
        var (board, solveSettings, timeSituation) = gameMode.CreateGame(settings);

        var                           solver = new Solver(wordList, solveSettings);
        ImmutableSortedSet<FoundWord> foundWords;

        if (savedGame == null)
            foundWords = ImmutableSortedSet<FoundWord>.Empty;
        else
            foundWords = savedGame.FoundWords.Select(solver.CheckLegal)
                .Where(x => x is not null)
                .ToImmutableSortedSet()!;

        MoggleState newState = new(
            board,
            solver,
            timeSituation,
            0,
            ImmutableList<Coordinate>.Empty,
            foundWords,
            ImmutableHashSet<FoundWord>.Empty,
            null,
            gameMode,
            settings
        );

        return newState;
    }

    public int MaxDimension => Math.Max(Board.Columns, Board.Rows);

    public MoveResult TryGetMoveResult(Coordinate coordinate)
    {
        if (TimeSituation is TimeSituation.Finished)
            return MoveResult.IllegalMove.Instance;

        if (TimeSituation.IsFinished)
            return new MoveResult.TimeElapsed(
                this with
                {
                    TimeSituation = TimeSituation.Finished.Instance,
                    ChosenPositions = ImmutableList<Coordinate>.Empty
                }
            );

        if (ChosenPositions.Any() && ChosenPositions.Last().Equals(coordinate))
        {
            return new MoveResult.WordAbandoned(
                this with { ChosenPositions = ImmutableList<Coordinate>.Empty }
            );
        }

        var index = ChosenPositions.LastIndexOf(coordinate);

        if (index >= 0)
            return new MoveResult.MoveRetraced(
                this with { ChosenPositions = ChosenPositions.Take(index + 1).ToImmutableList() }
            );

        if (!ChosenPositions.Any() || ChosenPositions.Last().IsAdjacent(coordinate))
        {
            var newChosenPositions = ChosenPositions.Add(coordinate);

            var word = string.Join(
                "",
                newChosenPositions.Select(GetLetterAtCoordinate).Select(x => x.WordText)
            );

            var foundWord = Solver.CheckLegal(word);

            if (foundWord is not null && !FoundWords.Contains(foundWord))
            {
                return new MoveResult.WordComplete(
                    this with
                    {
                        FoundWords = FoundWords.Add(foundWord),
                        ChosenPositions = newChosenPositions
                    },
                    foundWord
                );
            }

            return new MoveResult.WordContinued(this with { ChosenPositions = newChosenPositions });
        }

        return MoveResult.IllegalMove.Instance;
    }

    public Letter GetLetterAtCoordinate(Coordinate coordinate)
    {
        var newCoordinate = coordinate.Rotate(Board.MaxCoordinate, Rotation);
        return Board.GetLetterAtCoordinate(newCoordinate);
    }

    public int Score
    {
        get
        {
            return Words.Sum(s => s.Points);
        }
    }

    public int NumberOfWords => Words.Count();

    private IEnumerable<FoundWord> Words
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
