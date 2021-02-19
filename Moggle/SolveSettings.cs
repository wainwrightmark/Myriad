namespace Moggle
{

public record SolveSettings(int? MinWordLength, bool AllowTrueEquations, int? AllowedMathExpression)
{
    public bool AllowMath => AllowTrueEquations || AllowedMathExpression.HasValue;

    public bool AllowWords => MinWordLength.HasValue;

    public int MinimumTermLength => MinWordLength ?? 2;
}

}
