namespace Moggle
{

public record StringWord(string Text) : FoundWord(Text)
{
    public override string Display => Text;

    public override string Comparison => Text.ToLowerInvariant();

    public override int Points => ScoreWord(Text.Length);

    private static int ScoreWord(int length)
    {
        return length switch
        {
            < 3  => 0,
            3    => 1,
            4    => 1,
            5    => 2,
            6    => 3,
            7    => 4,
            >= 8 => 11
        };
    }
}

}