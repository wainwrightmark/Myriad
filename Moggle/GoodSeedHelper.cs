using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moggle
{
    public static class GoodSeedHelper
    {

        public static string GetGoodSeed()
        {
            var r = new Random();
            return GoodSeeds.Value[r.Next(GoodSeeds.Value.Count)];
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
