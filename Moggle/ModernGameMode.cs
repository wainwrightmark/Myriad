using System.Collections.Generic;
using System.Collections.Immutable;

namespace Moggle
{

public record ModernGameMode : BagGameMode
{
    private ModernGameMode() { }

    public static ModernGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Letters { get; } =
        "AAEEGNABBJOOACHOPSAFFKPSAOOTTWCIMOTUDEILRXDELRVYDISTTYEEGHNWEEINSUEHRTVWEIOSSTELRTTYHIMNUQHLNNRZ";

    /// <inheritdoc />
    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
    {
        var minWordLength = MinWordLength.Get(settings);

        return new SolveSettings(minWordLength, false, null);
    }

    public static readonly Setting.Integer MinWordLength = new(nameof(MinWordLength), 2, 8, 3);

    /// <inheritdoc />
    public override string GetDefaultLetters(int width, int height)
    {
        if (width == 4 && height == 4)
            return "welc2emomogg💖le💖";

        if (width == 3 && height == 3)
            return "letretfun";

        if (width == 5 && height == 5)
            return "heartemberabuseresintrend";

        if (width == 6 && height == 6)
            return "moggleogglemgglemoglemoglemoggemoggl";

        if (width == 7 && height == 7)
            return "bravadorenamedanalogyvaluersamoebasdegradeodyssey";

        return "moggle";

    }

    /// <inheritdoc />
    public override IEnumerable<Setting> Settings
    {
        get
        {
            foreach (var setting in base.Settings)
            {
                yield return setting;
                yield return MinWordLength;
            }
        }
    }

    /// <inheritdoc />
    public override string Name => "Modern";
}

}
