using System;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq.Extensions;

namespace Myriad
{

public record NumbersGameMode : NumberGameMode
{
    private NumbersGameMode() { }
    public static NumbersGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Numbers";

    /// <inheritdoc />
    public override bool ReverseAnimationOrder => true;

    /// <inheritdoc />
    public override Board GenerateCuratedRandomBoard(Random random)
    {
        var operators = "+++---**/^";
        var numbers   = "1122334455667788990";

        var opCount = new[] { 1, 2, 2, 3, 3, 4 }.RandomSubset(1, random).Single();

        var numCount = 9 - opCount;

        var chars = operators.RandomSubset(opCount, random)
            .Concat(numbers.RandomSubset(numCount, random));

        var letters = chars.Select(Letter.Create).ToImmutableArray();

        return new Board(letters, 3);
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
