using System;
using System.Collections.Generic;

namespace Moggle
{
    public static class GoodSeedHelper
    {

        public static string GetGoodSeed(Random random)
        {
            return GoodSeeds.Value[random.Next(GoodSeeds.Value.Count)];
        }


        public static readonly Lazy<IReadOnlyList<string>> GoodSeeds =
            new(() =>
                {

                    var words = Words.GoodSeeds.Split(
                        new []{'\r', '\n'},
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                    );

                    return words;
                }
            );
    }
}
