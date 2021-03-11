using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moggle.States;

namespace Moggle
{

public record SecretGameMode : IMoggleGameMode
{
    private SecretGameMode() { }
    public static SecretGameMode Instance { get; } = new();

    /// <inheritdoc />
    public string Name => "Secret";

    /// <inheritdoc />
    public MoggleBoard CreateBoard(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var wordsText = Words.Get(settings);
        var allWords  = Creator.GridCreator.GetAllWords(wordsText).ToList();
        var grid      = Creator.GridCreator.CreateNodeGrid(allWords, null, 10000);
        var random    = RandomHelper.GetRandom(wordsText);
        var board     = grid.ToMoggleBoard(() => ModernGameMode.Instance.GetRandomRune(random));

        return board;
    }

    /// <inheritdoc />
    public Solver CreateSolver(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var wordsText     = Words.Get(settings);
        //var minWordLength = MinWordLength.Get(settings);
        var allWords      = Creator.GridCreator.GetAllWords(wordsText).ToList();
        var solveSettings = new SolveSettings(allWords.Select(x=>x.Length).Append(3).Min(), false, null);
        var solver        = new Solver(wordList.Value.AddWords(allWords), solveSettings);
        return solver;
    }

    /// <inheritdoc />
    public TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings)
    {
        var ts = TimeSituation.Create(TimeSituation.Duration.Get(settings));
        return ts;
    }

    /// <inheritdoc />
    public Animation? GetAnimation(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var wordsText = Words.Get(settings);
        var allWords  = Creator.GridCreator.GetAllWords(wordsText).ToList();
        var grid      = Creator.GridCreator.CreateNodeGrid(allWords, null, 10000);
        var random    = RandomHelper.GetRandom(wordsText);

        var board = grid.ToMoggleBoard(() => ModernGameMode.Instance.GetRandomRune(random));

        if (Animate.Get(settings))
            return Animation.Create(allWords, board);

        return null;
    }

    public static readonly Setting.String Words =
        new(
            nameof(Words),
            @"\A(?:\w+)(?:\s\w+)*\Z",
            "horizontal dancing",
            "Words to appear in the grid"
        ) { GetRandomValue = GoodSeedHelper.GetGoodSeed };

    //public static readonly Setting.Integer MinWordLength = new(nameof(MinWordLength), 2, 8, 3);

    public static readonly Setting.Bool Animate = new(nameof(Animate), false);

    /// <inheritdoc />
    public IEnumerable<Setting> Settings
    {
        get
        {
            //yield return MinWordLength;
            yield return Words;
            yield return TimeSituation.Duration;
            yield return Animate;
        }
    }

    /// <inheritdoc />
    public FoundWordsData GetFoundWordsData(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList)
    {
        return new FoundWordsData.OpenSearchData(ImmutableDictionary<FoundWord, bool>.Empty);
    }
}

}
