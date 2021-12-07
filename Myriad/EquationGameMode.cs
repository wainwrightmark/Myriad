using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Myriad;

public record EquationGameMode : BagGameMode
{
    private EquationGameMode() { }
    public static EquationGameMode Instance { get; } = new ();

    /// <inheritdoc />
    public override string Name => "Equation";

    /// <inheritdoc />
    public override IEnumerable<Letter> LegalLetters { get; } = Letter.CreateFromString("=0123456789+-*/^!");

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
                ('=', 12),
                ('+', 6),
                ('*', 6),
                ('-', 4),
                ('/', 4) // Can't do divide yet - remainders are bad
            };

            foreach (var (c, number) in terms)
                sb.Append(new string(c, number));

            return sb.ToString();
        }
    }

    /// <inheritdoc />
    public override string GetDefaultLetters(int width, int height)
    {
        if (width == 3 && height == 3)
            return "123+=*654";

        if (width == 4 && height == 4)
            return "12345+-==*/64321";

        return "123+456*789";
    }

    /// <inheritdoc />
    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
    {
        return new(null, true, null);
    }
}