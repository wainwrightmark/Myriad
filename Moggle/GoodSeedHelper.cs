using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Moggle
{

public static class GoodSeedHelper
{
    public static string GetGoodSeed(Random random) =>
        GoodSeeds.GetRandomElement(random);

    public static string GetGoodRomanGame(Random random) =>
        GoodRomanGames.GetRandomElement(random);

    public static string GetGoodCenturyGame(Random random) => GoodCenturyGames.GetRandomElement(random);

    public static (string group, string grid, IReadOnlyCollection<string> words)
        GetChallengeGame(string name) => GoodChallengeGames.Value.Single(x=>x.group.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static T GetRandomElement<T>(this Lazy<IReadOnlyList<T>> stuff, Random random)
    {
        var n = random.Next(stuff.Value.Count);
        var e = stuff.Value[n];
        return e;
    }

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

    public static readonly
        Lazy<IReadOnlyList<(string group, string grid, IReadOnlyCollection<string> words)>>
        GoodChallengeGames =
            new(
                () =>
                {
                    var lines = GoodGames.ChallengeGames.Split(
                            new[] { '\r', '\n' },
                            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
                        )
                        .Select(CreateChallengeGame)
                        .ToList();

                    return lines;

                    static (string group, string grid, IReadOnlyCollection<string> words)
                        CreateChallengeGame(string arg)
                    {
                        var m = ChallengeGameRegex.Match(arg);

                        var words = m.Groups["words"]
                            .Value.Split(",")
                            .Select(x => x.Trim().ToUpperInvariant())
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        return (m.Groups["group"].Value, m.Groups["grid"].Value.ToUpperInvariant(), words);
                    }
                }
            );

    private static readonly Regex ChallengeGameRegex = new(
        @"\A(?<group>\w+);\s*(?<grid>[a-zA-Z0-9_\-]+);\s*(?<words>(?:\w+,?\s*)*)\Z",
        RegexOptions.Compiled
    );
}

}
