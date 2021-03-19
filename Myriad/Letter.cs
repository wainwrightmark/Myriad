using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myriad
{

public record Letter(string ButtonText, string WordText, bool IsLegal)
{
    public static Letter Create(Rune rune)
    {
        if (rune.ToString().Equals("Q") || rune.ToString().Equals("q"))//Weird special case for Q
            return new Letter("Qᵤ", "QU", true);

        if (rune.ToString().Equals("_"))
            return new Letter("_", "_", false);

        return new Letter(rune.ToString().ToUpper(), rune.ToString().ToUpper(), true);
    }

    public static Letter Create(char c)
    {
        return Create(new Rune(c));
    }

    public static IEnumerable<Letter> CreateFromString(string s) => s.EnumerateRunes().Select(Create);
}

}
