using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Moggle
{

public record TargetWord(string Text, string Group) : FoundWord(Text)
{
    /// <inheritdoc />
    public override string Display => Text;

    /// <inheritdoc />
    public override string Comparison => Text;

    /// <inheritdoc />
    public override string AnimationString => Text;

    /// <inheritdoc />
    public override int Points => 1;

    /// <inheritdoc />
    protected override Type EqualityContract { get; } = typeof(FoundWord);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public interface IMoggleGameMode
{
    string Name { get; }

    MoggleBoard CreateBoard(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    Solver CreateSolver(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    TimeSituation CreateTimeSituation(ImmutableDictionary<string, string> settings);

    Animation? GetAnimation(ImmutableDictionary<string, string> settings, Lazy<WordList> wordList);

    IEnumerable<Setting> Settings { get; }

    IReadOnlyCollection<TargetWord>? GetTargetWords(
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

}
