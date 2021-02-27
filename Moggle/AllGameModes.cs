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
            ModernGameMode.Instance,
            CenturyGameMode.Instance,
            ClassicGameMode.Instance,
            EquationGameMode.Instance,

            FixedGameMode.Instance,
            SecretGameMode.Instance,
            RomanGameMode.Instance
        }.ToDictionary(x=>x.Name, StringComparer.OrdinalIgnoreCase);

    public static IMoggleGameMode? CreateFromString(string s)
    {
        if(Modes.TryGetValue(s, out var m))
            return m;
        return null;
    }
}

}
