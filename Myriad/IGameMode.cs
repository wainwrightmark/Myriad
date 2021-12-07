using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Myriad.States;

namespace Myriad;

public interface IGameMode
{
    string Name { get; }

    Board CreateBoard(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    Solver CreateSolver(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings);

    Animation? GetAnimation(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    abstract IEnumerable<Letter> LegalLetters { get; } //Used for generating

    IEnumerable<Setting> Settings { get; }

    public FoundWordsData GetFoundWordsData(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList);

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