namespace Moggle
{

public record Letter(string ButtonText, string WordText)
{
    public static Letter Create(char c)
    {
        if (c.Equals('Q') || c.Equals('q'))
            return new Letter("Qᵤ", "QU");

        if (c.Equals('*'))
            return new Letter("😊", "😊");

        return new Letter(c.ToString().ToUpper(), c.ToString().ToUpper());
    }
}

}
