using System.Collections.Generic;
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
                ('/', 2)
            };

            foreach (var (c, number) in terms)
                sb.Append(new string(c, number));

            return sb.ToString();
        }
    }

    /// <inheritdoc />
    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
    {
        var target = Target.Get(settings);

        return new SolveSettings(null, false, target);
    }

    public static readonly Setting.Integer Target = new(nameof(Target), 0, int.MaxValue, 42);

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
            yield return Target;
        }
    }
}

}
