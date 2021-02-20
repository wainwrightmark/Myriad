using System.Collections.Generic;
using System.Collections.Immutable;

namespace Moggle
{

public interface IMoggleGameMode
{
        string Name { get; }

    (MoggleBoard board, SolveSettings solveSettings, TimeSituation TimeSituation) CreateGame(
        ImmutableDictionary<string, string> settings);

    IEnumerable<Setting> Settings { get; }

    bool AreSettingsValid(ImmutableDictionary<string, string> valueDictionary)
    {
        foreach (var setting in Settings)
        {
            if (valueDictionary.TryGetValue(setting.Name, out var value) && setting.IsValid(value))
                continue;

            return false;
        }

        return true;
    }
}

}
