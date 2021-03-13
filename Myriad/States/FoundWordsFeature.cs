using System;
using System.Collections.Immutable;
using Fluxor;

namespace Myriad.States
{

public static class Defaults
{
    public static IGameMode Mode => CenturyGameMode.Instance;

    public static ImmutableDictionary<string, string> Settings =>
        ImmutableDictionary<string, string>.Empty;

    public static Lazy<WordList> WordList => Myriad.WordList.LazyInstance;
}

public class FoundWordsFeature : Feature<FoundWordsState>
{
    /// <inheritdoc />
    public override string GetName()
    {
        return "Unchecked Words";
    }

    /// <inheritdoc />
    protected override FoundWordsState GetInitialState()
    {
        var data = Defaults.Mode.GetFoundWordsData(Defaults.Settings, Defaults.WordList);

        return new FoundWordsState(data);
    }
}

}
