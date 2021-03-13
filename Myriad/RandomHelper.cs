using System;

namespace Myriad
{

public static class RandomHelper
{
    public static Random GetRandom(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return new Random(0);

        var current = 1;

        foreach (var v in s.Trim().ToLowerInvariant())
        {
            current += v;
            current *= v;
        }

        return new Random(current);
    }
}

}
