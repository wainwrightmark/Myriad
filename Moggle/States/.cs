//using System;
//using System.Collections.Immutable;

//namespace Moggle.States
//{

//public record MoggleState( //TODO move each of these to become its own object
//    MoggleBoard Board,
//    Solver Solver)
//{
//    public static readonly MoggleState DefaultState =
//        StartNewGame(
//            WordList.LazyInstance,
//            CenturyGameMode.Instance,
//            ImmutableDictionary<string, string>.Empty
//        );

//    public static MoggleState StartNewGame(
//        WordList wordList,
//        IMoggleGameMode gameMode,
//        ImmutableDictionary<string, string> settings) => StartNewGame(
//        new Lazy<WordList>(() => wordList),
//        gameMode,
//        settings
//    );

//    public static MoggleState StartNewGame(
//        Lazy<WordList> wordList,
//        IMoggleGameMode gameMode,
//        ImmutableDictionary<string, string> settings)
//    {
//        var (board, solver) = gameMode.CreateGame(settings, wordList);

//        MoggleState newState = new(
//            board,
//            solver
//        );

//        return newState;
//    }
//}

//}
