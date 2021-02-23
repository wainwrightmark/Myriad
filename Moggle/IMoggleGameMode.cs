using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Moggle
{

public interface IMoggleGameMode
{
    string Name { get; }

    (MoggleBoard board, SolveSettings solveSettings, TimeSituation TimeSituation) CreateGame(
        ImmutableDictionary<string, string> settings);

    IEnumerable<Setting> Settings { get; }

    public IEnumerable<(string key, string value)> FilterSettings(IReadOnlyDictionary<string, string> dict)
    {
        foreach (var setting in Settings)
        {
            if (dict.TryGetValue(setting.Name, out var valString) && setting.IsValid(valString)
             && valString != setting.DefaultString)
                yield return (setting.Name, valString);
        }
    }

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
