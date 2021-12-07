using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq.Extensions;

namespace Myriad;

//public record MiniNumbersGameMode : NumbersGameMode
//{
//    private MiniNumbersGameMode() { }
//    public static MiniNumbersGameMode Instance { get; } = new();

//    /// <inheritdoc />
//    public override int Columns { get; } = 2;

//    /// <inheritdoc />
//    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
//    {
//        return new(null, false, (1, 10));
//    }
//}

public record NumbersGameMode : NumberGameMode
{
    protected NumbersGameMode() { }
    public static NumbersGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Numbers";

    /// <inheritdoc />
    public override bool ReverseAnimationOrder => true;

    /// <inheritdoc />
    public override IEnumerable<Letter> LegalLetters { get; } =
        Letter.CreateFromString("0123456789+-*/^!");

    /// <inheritdoc />
    public override Board GenerateCuratedRandomBoard(Random random)
    {
        var operators = "+++---**/^!!";
        var numbers   = "1122334455667788990";

        var opCount = new[] { 1, 2, 2, 3, 3, 4 }.RandomSubset(1, random).Single();

        var numCount = (Columns * Columns) - opCount;

        var chars = operators.RandomSubset(opCount, random)
            .Concat(numbers.RandomSubset(numCount, random))
            .Shuffle(random);

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