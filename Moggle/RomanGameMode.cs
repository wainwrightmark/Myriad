using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;

namespace Moggle
{

public record RomanGameMode : WhitelistGameMode
{
    private RomanGameMode() { }
    public static RomanGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Roman";

    public override MoggleBoard GenerateRandomBoard(Random random)
    {
        var possibleLetters = "IVXLCDM+-*/^";

        var a = MoreEnumerable.Random(random, 0, possibleLetters.Length)
            .Select(x => possibleLetters[x])
            .Select(Letter.Create)
            .Take(9)
            .ToImmutableArray();

        return new MoggleBoard(a, 3);
    }

    /// <inheritdoc />
    public override MoggleBoard GenerateCuratedRandomBoard(Random random)
    {
        var chars = "+-*".RandomSubset(2, random)
            .Concat("IIIIIIVVVXXX".RandomSubset(7, random));

        var letters = chars.Select(Letter.Create).ToImmutableArray();

        return new MoggleBoard(letters, 3);
    }

    /// <inheritdoc />
    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
    {
        return new(null, false, (1, 100));
    }

    /// <inheritdoc />
    public override bool ReverseAnimationOrder => true;

    /// <inheritdoc />
    public override ImmutableArray<Letter> GetLetters(Random random)
    {
        var lettersString = GoodSeedHelper.GetGoodRomanGame(random);
        var array         = lettersString.EnumerateRunes().Select(Letter.Create).ToImmutableArray();

        return array;
    }

    /// <inheritdoc />
    public override int Columns => 3;

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
    public override IReadOnlyCollection<TargetWord> GetTargetWords(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        return Enumerable.Range(1, 100)
            .Select(x => new TargetWord(x.ToString(), (((x % 100) / 10) * 10).ToString()))
            .ToList();
    }
}

}
