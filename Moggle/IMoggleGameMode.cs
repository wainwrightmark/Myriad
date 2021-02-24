using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Moggle
{

public interface IMoggleGameMode
{
    string Name { get; }

    (MoggleBoard board, Solver Solver)
        CreateGame(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings);

    Animation? GetAnimation(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    IEnumerable<Setting> Settings { get; }

    public IEnumerable<(string key, string value)> FilterSettings(
        IReadOnlyDictionary<string, string> dict)
    {
        foreach (var setting in Settings)
        {
            if (dict.TryGetValue(setting.Name, out var valString) && setting.IsValid(valString)
             && valString != setting.DefaultString)
                yield return (setting.Name, valString);
        }
    }
}

}
