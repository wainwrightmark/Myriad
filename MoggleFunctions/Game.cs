using System;
using System.Collections.Generic;
using System.Text;

namespace MoggleFunctions
{
    public class Game
    {

        public string Id { get; set; }

        public string GameId { get; set; }

        public string Creator {get; set; }

        public int Width {get; set; }
        public int Height {get; set; }
        public int Duration {get; set; }
        public bool Classic {get; set; }

        public string GridLetters { get; set; }
    }

public class Result
{
    public string GameId {get; set; }

    public string Player { get; set; }
    public string[] Words { get; set; }
}

}
