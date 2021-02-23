using System.Collections.Generic;
using System.Linq;

namespace Moggle
{
    public class SavedGame
    {
        public string GameString { get; set; }

        public string[] FoundWords { get; set; }

        public static SavedGame Create(MoggleState state)
        {
            var gameString = CreateGameString(state.LastGameMode, state.LastSettings);

            return new()
            {
                GameString = gameString,
                FoundWords = state.FoundWords.Select(x => x.Text).ToArray()
            };
        }

        public static string CreateGameString(IMoggleGameMode mode, IReadOnlyDictionary<string, string> settings)
        {
            var uri = $"mode={mode.Name}";
            foreach(var (key, value) in mode.FilterSettings(settings))
            {
                uri += $"&{key.ToLowerInvariant()}={value}";
            }

            return uri;
        }
    }
}
