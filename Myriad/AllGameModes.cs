using System;
using System.Collections.Generic;
using System.Linq;

namespace Myriad
{

public static class AllGameModes
{
    public static readonly IReadOnlyDictionary<string, IGameMode> Modes =
        new List<IGameMode>()
        {
            NumbersGameMode.Instance,
            WordsGameMode.Instance,
            ChallengeGameMode.Instance,
            CustomGameMode.Instance,
            SecretGameMode.Instance,
            RomanGameMode.Instance,
            EquationGameMode.Instance,
        }.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public static IGameMode? CreateFromString(string s)
    {
        if (Modes.TryGetValue(s, out var m))
            return m;

        return null;
    }
}

}
