namespace Myriad;

public record SolveSettings(int? MinWordLength, bool AllowTrueEquations, (int Min, int Max)? MathExpressionsRange)
{
    public bool AllowMath => AllowTrueEquations || MathExpressionsRange is not null;

    public bool AllowWords => MinWordLength.HasValue;

    public int MinimumTermLength => MinWordLength ?? 1;
}