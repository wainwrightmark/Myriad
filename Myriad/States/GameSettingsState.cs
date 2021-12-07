using System.Collections.Generic;
using System.Collections.Immutable;

namespace Myriad.States;

public record GameSettingsState(
    IGameMode LastGameMode,
    ImmutableDictionary<string, string> LastSettings)
{
    public string GameString => CreateGameString(LastGameMode, LastSettings);

    public static string CreateGameString(
        IGameMode mode,
        IReadOnlyDictionary<string, string> settings)
    {
        var uri = $"mode={mode.Name}";

        foreach (var (key, value) in mode.FilterSettings(settings))
        {
            uri += $"&{key.ToLowerInvariant()}={value}";
        }

        return uri;
    }
}