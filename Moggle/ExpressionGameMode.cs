﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Moggle
{

public record ExpressionGameMode : BagGameMode
{
    private ExpressionGameMode() { }
    public static ExpressionGameMode Instance { get; } = new();

    /// <inheritdoc />
    public override string Name => "Expression";

    /// <inheritdoc />
    public override string Letters
    {
        get
        {
            var sb = new StringBuilder();

            var terms = new List<(char c, int number)>()
            {
                ('1', 4),
                ('2', 4),
                ('3', 4),
                ('4', 4),
                ('5', 3),
                ('6', 3),
                ('7', 3),
                ('8', 2),
                ('9', 2),
                ('0', 1),
                ('+', 6),
                ('*', 6),
                ('-', 4),
                //('/', 2) Can't do divide yet - remainders are bad
            };

            foreach (var (c, number) in terms)
                sb.Append(new string(c, number));

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

    public static readonly Setting.Integer Minimum = new(nameof(Minimum), int.MinValue, int.MaxValue, 1);
    public static readonly Setting.Integer Maximum = new(nameof(Maximum), 0, int.MaxValue, 99);

        /// <inheritdoc />
        public override string GetDefaultLetters(int width, int height)
        {
            if (width == 3 && height == 3)
                return "123+7*654";

            if (width == 4 && height == 4)
                return "12345+-87*/64321";

            return "123+4=6*789";
        }

        /// <inheritdoc />
        public override IEnumerable<Setting> ExtraSettings
    {
        get
        {
            yield return Minimum;
            yield return Maximum;
        }
    }
}

}