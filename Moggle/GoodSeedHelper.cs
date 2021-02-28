using System;
using System.Collections.Generic;

namespace Moggle
{

public static class GoodSeedHelper
{
    public static string GetGoodSeed(Random random) => GoodSeeds.Value[random.Next(GoodSeeds.Value.Count)];
    public static string GetGoodRomanGame(Random random) => GoodRomanGames.Value[random.Next(GoodRomanGames.Value.Count)];
    public static string GetGoodCenturyGame(Random random) => GoodCenturyGames.Value[random.Next(GoodCenturyGames.Value.Count)];

    public static readonly Lazy<IReadOnlyList<string>> GoodSeeds =
        new(
            () =>
            {
                var words = GoodGames.GoodSeeds.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                );

                return words;
            }
        );


        public static readonly Lazy<IReadOnlyList<string>> GoodCenturyGames =
            new(
                () =>
                {
                    var words = GoodGames.Century.Split(
                        new[] { '\r', '\n' },
                        StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                    );

                    return words;
                }
            );


        public static readonly Lazy<IReadOnlyList<string>> GoodRomanGames =
        new(
            () =>
            {
                var words = GoodGames.Roman.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                );

                return words;
            }
        );



        //public static readonly Lazy<IReadOnlyList<string>> GoodExpressionSeeds =
        //    new(
        //        () =>
        //        {
        //            var words = Words.GoodExpressionSeeds.Split(
        //                new[] { '\r', '\n' },
        //                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
        //            );

        //            return words;
        //        }
        //    );
    }

}
