using System;
using System.Collections.Generic;
using System.Linq;

namespace Moggle
{

public static class AllGameModes
{
    public static readonly IReadOnlyDictionary<string, IMoggleGameMode> Modes =
        new List<IMoggleGameMode>()
        {
            CenturyGameMode.Instance,
            ModernGameMode.Instance,
            ClassicGameMode.Instance,
            ChallengeGameMode.Instance,
            FixedGameMode.Instance,
            SecretGameMode.Instance,
            RomanGameMode.Instance,
            EquationGameMode.Instance,
        }.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

    public static IMoggleGameMode? CreateFromString(string s)
    {
        if (Modes.TryGetValue(s, out var m))
            return m;

        return null;
    }
}

}
