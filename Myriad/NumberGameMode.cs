using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Myriad.States;

namespace Myriad
{

public abstract record NumberGameMode : WhitelistGameMode
{
    /// <inheritdoc />
    public override SolveSettings GetSolveSettings(ImmutableDictionary<string, string> settings)
    {
        return new(null, false, (1, 100));
    }

    /// <inheritdoc />
    public override bool ReverseAnimationOrder => true;

    /// <inheritdoc />
    public override IEnumerable<Setting> Settings
    {
        get
        {
            yield return Seed;
            yield return AnimateSetting;
        }
    }

    /// <inheritdoc />
    public override FoundWordsData GetFoundWordsData(
        ImmutableDictionary<string, string> settings,
        Lazy<WordList> wordList)
    {
        var words = Enumerable.Range(1, 100)
            .Select(x => (word: x.ToString(), group: GetGroup(x)))
            .ToList();

        return new FoundWordsData.TargetWordsData(
            words.ToImmutableDictionary(x => x.word, x => (x.group, null as FoundWord))
        );

        static FoundWordsData.TargetWordGroup GetGroup(int i)
        {
            var tens = (i / 10) * 10;

            return TargetWordGroups[tens];
        }
    }

    private static readonly IReadOnlyDictionary<int, FoundWordsData.TargetWordGroup>
        TargetWordGroups =
            new List<FoundWordsData.TargetWordGroup>()
            {
                new("Units", 0, false),
                new("Tens", 10, false),
                new("Twenties", 20, false),
                new("Thirties", 30, false),
                new("Forties", 40, false),
                new("Fifties", 50, false),
                new("Sixties", 60, false),
                new("Seventies", 70, false),
                new("Eighties", 80, false),
                new("Nineties", 90, false),
                new("One Hundred", 100, false),
            }.ToDictionary(x => x.Order);


    /// <inheritdoc />
    public override int Columns => 3;
}

}
