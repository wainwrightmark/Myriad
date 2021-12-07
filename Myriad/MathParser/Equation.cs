namespace Myriad.MathParser;

public record Equation(int? Left, int? Right)
{
    public bool IsValid
    {
        get
        {
            if (Left.HasValue && Right.HasValue)
            {
                return Left.Value == Right.Value;
            }

            return false;
        }
    }
}