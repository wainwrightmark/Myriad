using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Moggle
{

public record RomanGameMode : BagGameMode
{
    private RomanGameMode() { }
    public static RomanGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Roman";

    /// <inheritdoc />
    public override string Letters
    {
        get
        {
            var sb = new StringBuilder();

            var terms = new List<(char c, int number)>() //TODO find best ratios
            {
                ('i', 16),
                ('v', 4),
                ('x', 2),
                ('l', 1),
                ('c', 1),
                ('d', 0),
                ('m', 0),
                ('+', 3),
                ('*', 3),
                ('-', 2),
                ('/', 1) // Can't do divide yet - remainders are bad
            };

            foreach (var (c, number) in terms)
                sb.Append(new string(c, number)); //TODO find optimal numbers

            return sb.ToString();
        }
    }

    /// <inheritdoc />
    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
    {
        var min = Minimum.Get(settings);
        var max = Maximum.Get(settings);

        return new SolveSettings(null, false, (min, max));
    }

    /// <inheritdoc />
    public override bool ReverseAnimationOrder => true;

    public static readonly Setting.Integer Minimum = new(
        nameof(Minimum),
        int.MinValue,
        int.MaxValue,
        1
    );

    public static readonly Setting.Integer Maximum = new(nameof(Maximum), 0, int.MaxValue, 100);

    /// <inheritdoc />
    public override string GetDefaultLetters(int width, int height)
    {
        if (width == 3 && height == 3)
            return "VII-*+IIX";

        return "VII-*+IIX";
    }

    /// <inheritdoc />
    public override Setting.Integer DurationSetting => TimeSituation.Duration with { Default = -1 };

    /// <inheritdoc />
    public override Setting.Integer Width => base.Width with { Default = 3 };

    public override Setting.Integer Height => base.Height with { Default = 3 };

    /// <inheritdoc />
    public override Setting.String Seed => base.Seed with
    {
        GetRandomValue = GoodSeedHelper.GetGoodExpressionSeed
    };

    /// <inheritdoc />
    public override IEnumerable<Setting> Settings
    {
        get
        {
            yield return Seed;
            yield return Width;
            yield return Height;
            yield return Minimum;
            yield return Maximum;
            yield return DurationSetting;
            yield return AnimateSetting;
        }
    }
}

}
