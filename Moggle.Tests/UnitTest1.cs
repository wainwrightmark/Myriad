using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Moggle.Tests
{
    public class UnitTest1
    {
        public ITestOutputHelper TestOutputHelper { get; }

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            _solver = new Lazy<Solver>(Solver.FromResourceFile);
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello")]
        [InlineData("World")]
        public async Task SameSeedShouldProduceSameGame(string seed)
        {
            var board1 = MoggleState.DefaultState.StartNewGame(seed, 5, 5, false, 120);

            await Task.Delay(100);

            var board2 = MoggleState.DefaultState.StartNewGame(seed, 5, 5, false, 120);

            var b1String = string.Join(", ", board1.Board.Positions.Select(x => x.ToString()));
            var b2String = string.Join(", ", board2.Board.Positions.Select(x => x.ToString()));

            TestOutputHelper.WriteLine(b1String);
            TestOutputHelper.WriteLine(b2String);

            board1.Board.Positions.Should().BeEquivalentTo(board2.Board.Positions);
        }


        [Theory]
        [InlineData("")]
        [InlineData("mark")]
        [InlineData("ruth")]
        public void Test1(string key)
        {
            var s = MoggleState.DefaultState.StartNewGame(key, 4, 4, false, 120).Board;


            s.Height.Should().Be(4);
            s.Width.Should().Be(4);

            s.Positions.Should().HaveCount(16);
            s.Dice.Should().HaveCount(16);

            for (var r = 0; r < s.Height; r++)
            {
                for (var c = 0; c < s.Width; c++)
                {
                    var val = s.GetLetterAtCoordinate(new Coordinate(r, c));

                    TestOutputHelper.WriteLine($"{r}, {c}: {val}");
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestLetterFrequencies(bool classic)
        {

            var dict = new ConcurrentDictionary<Letter, int>();

            for (var i = 0; i < 1000; i++)
            {
                var s = MoggleState.DefaultState.StartNewGame(i.ToString(), 4, 4, classic, 120);


                for (var j = 0; j < 16; j++)
                {
                    var l = s.Board.GetLetterAtIndex(j);
                    dict.AddOrUpdate(l, 1,(x, i) => i + 1);
                }
            }


            dict.Count.Should().Be(26);

            foreach (var (key, value) in dict.OrderBy(x=>x.Key.WordText))
            {
                TestOutputHelper.WriteLine($"{key.WordText}: {value}");
            }
        }


        private readonly Lazy<Solver> _solver;

        [Theory]
        [InlineData("abc", false, 4,4)]
        [InlineData("ham", false, 4,4)]
        [InlineData("alphabet", false, 4,4)]
        [InlineData("concurrent", false, 4,4)]
        [InlineData("hyperventilate", false, 4,4)]
        [InlineData("my hovercraft is full of eels", false, 4,4)]
        [InlineData("my hovercraft is full of eels", false, 5,5)]
        [InlineData("my hovercraft is full of eels", false, 6,6)]
        public void TestSolver(string seed, bool classic, int height, int width)
        {
            var state = MoggleState.DefaultState.StartNewGame(seed, width, height, classic, 120);

            var sw = Stopwatch.StartNew();
            var words = _solver.Value.GetPossibleWords(state.Board).ToList();
            sw.Stop();

            TestOutputHelper.WriteLine(sw.ElapsedMilliseconds + "ms");
            TestOutputHelper.WriteLine(words.Count + "Words");

            TestOutputHelper.WriteLine("");
            TestOutputHelper.WriteLine("");

            words.Should().NotBeEmpty();

            foreach (var word in words)
            {
                TestOutputHelper.WriteLine(word);
            }

        }

        [Theory]
        [InlineData(10000, false, 3,3)]
        [InlineData(10000, true, 3,3)]
        [InlineData(10000, false, 4,4)]
        [InlineData(10000, true, 4,4)]
        public void FindBestGrids(int trials, bool classic, int height, int width)
        {
            var bestSeed  = 0;
            var bestScore = 0;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < trials; i++)
            {
                var state = MoggleState.DefaultState.StartNewGame(i.ToString(), width, height, classic, 120);
                var words = _solver.Value.GetPossibleWords(state.Board).ToList();
                var score = words.Select(x => x.Length).Select(MoggleState.ScoreWord).Sum();

                if (score > bestScore)
                {
                    bestScore = score;
                    bestSeed  = i;
                }

            }
            sw.Stop();

            TestOutputHelper.WriteLine(sw.ElapsedMilliseconds + "ms");

            TestOutputHelper.WriteLine($"Seed: {bestSeed}");
            TestOutputHelper.WriteLine($"Score: {bestScore}");
        }


        [Theory]
        [InlineData(1000, false, 3, 3)]
        [InlineData(1000, true, 3, 3)]
        [InlineData(1000, false, 4, 4)]
        [InlineData(1000, true,  4, 4)]
        [InlineData(1000, false, 5, 5)]
        [InlineData(1000, true,  5, 5)]
        [InlineData(1000, false, 6, 6)]
        [InlineData(1000, true,  6, 6)]
        public void FindAverageWords(int numberOfTests, bool classic, int height, int width)
        {
            long totalWords = 0;
            long totalScore = 0;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < numberOfTests; i++)
            {
                var state = MoggleState.DefaultState.StartNewGame(i.ToString(), width, height, classic, 120);
                var words = _solver.Value.GetPossibleWords(state.Board).ToList();
                totalWords += words.Count;
                totalScore += words.Select(x => x.Length).Select(MoggleState.ScoreWord).Sum();
            }
            sw.Stop();

            TestOutputHelper.WriteLine(sw.ElapsedMilliseconds + "ms");
            var averageWords = totalWords / numberOfTests;
            var averageScore = totalScore / numberOfTests;

            TestOutputHelper.WriteLine($"AverageWords: {averageWords}");
            TestOutputHelper.WriteLine($"AverageScore: {averageScore}");

        }

        [Fact]
        public void GenerateWordList()
        {
            foreach (var word in _solver.Value.LegalWords.OrderBy(x=>x))
            {
                TestOutputHelper.WriteLine(word);
            }
        }


    }
}
