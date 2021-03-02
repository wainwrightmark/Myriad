using System;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq.Extensions;

namespace Moggle
{

public record CenturyGameMode : NumberGameMode
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
    public override MoggleBoard GenerateCuratedRandomBoard(Random random)
    {
        var operators = "+++---**/^";
        var numbers   = "1122334455667788990";

        var opCount = new[] { 1, 2, 2, 3, 3, 4 }.RandomSubset(1, random).Single();

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
}

}
