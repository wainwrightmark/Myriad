using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moggle.States;
using MoreLinq.Extensions;

namespace Moggle
{

public record CenturyGameMode : WhitelistGameMode
{
    private CenturyGameMode() { }
    public static CenturyGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Century";

    public override MoggleBoard GenerateRandomBoard(Random random)
    {
        var possibleLetters = "1234567890+-*/^";

        var a = MoreLinq.MoreEnumerable
            .Random(random, 0, possibleLetters.Length)
            .Select(x => possibleLetters[x])
            .Select(Letter.Create)
            .Take(9)
            .ToImmutableArray();

        return new MoggleBoard(a, 3);
    }

    /// <inheritdoc />
    public override int Columns => 3;

    /// <inheritdoc />
    public override MoggleBoard GenerateCuratedRandomBoard(Random random)
    {
        var operators = "+++---**/^";
        var numbers   = "1122334455667788990";

        var opCount = 2;

        opCount = new[] { 1, 2, 2, 3, 3, 4 }.RandomSubset(1, random).Single();

        var numCount = 9 - opCount;

        var chars = operators.RandomSubset(opCount, random)
            .Concat(numbers.RandomSubset(numCount, random));

        var letters = chars.Select(Letter.Create).ToImmutableArray();

        return new MoggleBoard(letters, 3);
    }

    /// <inheritdoc />
    public override ImmutableArray<Letter> GetLetters(Random random)
    {
        var lettersString = GoodSeedHelper.GetGoodCenturyGame(random);
        var array         = lettersString.EnumerateRunes().Select(Letter.Create).ToImmutableArray();

        return array;
    }

    /// <inheritdoc />
    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
    {
        return new(null, false, (1, 100));
    }

    /// <inheritdoc />
    public override bool ReverseAnimationOrder => true;

    /// <inheritdoc />
    public override IEnumerable<Setting> Settings
    {
        get
        {
            yield return Seed;
            yield return AnimateSetting;
        }
    }

    /// <inheritdoc />
    public override FoundWordsData GetFoundWordsData(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var words = Enumerable.Range(1, 100)
            .Select(x => (word: x.ToString(), group: (((x % 100) / 10) * 10).ToString()))
            .ToList();

        return new FoundWordsData.TargetWordsData(
            words.ToImmutableDictionary(x => x.word, x => (x.group, null as FoundWord))
        );
    }
}

}
