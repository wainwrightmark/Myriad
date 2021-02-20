using System;
using System.Collections.Generic;

namespace Moggle
{

public static class GoodSeedHelper
{
    public static string GetGoodSeed(Random random) => GoodSeeds.Value[random.Next(GoodSeeds.Value.Count)];

    public static string GetGoodExpressionSeed(Random random) => GoodExpressionSeeds.Value[random.Next(GoodExpressionSeeds.Value.Count)];

    public static readonly Lazy<IReadOnlyList<string>> GoodSeeds =
        new(
            () =>
            {
                var words = Words.GoodSeeds.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                );

                return words;
            }
        );

    public static readonly Lazy<IReadOnlyList<string>> GoodExpressionSeeds =
        new(
            () =>
            {
                var words = Words.GoodExpressionSeeds.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                );

                return words;
            }
        );
}

}
