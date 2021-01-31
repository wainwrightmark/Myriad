using System.Text;

namespace Moggle
{

public record Letter(string ButtonText, string WordText)
{
    public static Letter Create(Rune rune)
    {
        if (rune.ToString().Equals("Q") || rune.ToString().Equals("q"))//Weird special case for Q
            return new Letter("Qᵤ", "QU");

        return new Letter(rune.ToString().ToUpper(), rune.ToString().ToUpper());
    }
}

}
