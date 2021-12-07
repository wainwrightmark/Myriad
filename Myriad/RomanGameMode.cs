using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MoreLinq;

namespace Myriad;

public record RomanGameMode : NumberGameMode
{
    private RomanGameMode() { }
    public static RomanGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Roman";

    /// <inheritdoc />
    public override bool ReverseAnimationOrder => true;

    /// <inheritdoc />
    public override IEnumerable<Letter> LegalLetters { get; } =
        Letter.CreateFromString("IVXCD+-*/^!");

    /// <inheritdoc />
    public override Board GenerateCuratedRandomBoard(Random random)
    {
        var opNumber = random.Next(1, 4);

        var chars = "+-*".RandomSubset(opNumber, random)
            .Concat("IIIIIIVVVXXXLC".RandomSubset(9 - opNumber, random))
            .Shuffle(random);

        var letters = chars.Select(Letter.Create).ToImmutableArray();

        return new Board(letters, 3);
    }

    /// <inheritdoc />
    public override ImmutableArray<Letter> GetLetters(Random random)
    {
        var lettersString = GoodSeedHelper.GetGoodRomanGame(random);
        var array         = lettersString.EnumerateRunes().Select(Letter.Create).ToImmutableArray();

        return array;
    }
}