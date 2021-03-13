using System.Collections.Generic;
using System.Collections.Immutable;

namespace Myriad
{

public record ClassicGameMode : BagGameMode
{
    private ClassicGameMode() { }

    public static ClassicGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Letters { get; } =
        "AACIOTABILTYABJMOQACDEMPACELRSADENVZAHMORSBIFORXDENOSWDKNOTUEEFHIYEGKLUYEGINTVEHINPSELPSTUGILRUW";

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
        return ModernGameMode.Instance.GetDefaultLetters(width, height);
    }

    /// <inheritdoc />
    public override IEnumerable<Setting> Settings
    {
        get
        {
            foreach (var setting in base.Settings)
                yield return setting;

            yield return MinWordLength;
        }
    }

    /// <inheritdoc />
    public override string Name => "Classic";
}

}
