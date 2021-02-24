using System.Linq;
using Moggle.States;

namespace Moggle
{
    public class SavedGame
    {
        public string GameString { get; set; }

        public string[] FoundWords { get; set; }

        public static SavedGame Create(string gameString, MoggleState state)
        {
            return new()
            {
                GameString = gameString,
                FoundWords = state.FoundWords.Select(x => x.Text).ToArray()
            };
        }


    }
}
