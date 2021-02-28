using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moggle.States;
using MoreLinq;
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
            _wordList = new Lazy<WordList>(WordList.FromResourceFile);
        }

        private readonly Lazy<WordList> _wordList;


        private MoggleState CreateFromSeed(string seed, bool classic = false, int width = 4, int height = 4)
        {
            IMoggleGameMode mode = classic ? ClassicGameMode.Instance : ModernGameMode.Instance;

            return MoggleState.StartNewGame(
                _wordList.Value,
                mode,
                ImmutableDictionary.CreateRange(
                    new[]
                    {
                        new KeyValuePair<string, string>(
                            ModernGameMode.Instance.Seed.Name,
                            seed
                        ),
                        new KeyValuePair<string, string>(
                            ModernGameMode.Instance.Width.Name,
                            width.ToString()
                        ),
                        new KeyValuePair<string, string>(
                            ModernGameMode.Instance.Height.Name,
                            height.ToString()
                        ),
                    }
                )
            );
        }


        private MoggleState CreateMathStateFromSeed(
            string seed,
            bool equation = false,
            int width = 4,
            int height = 4)
        {
            IMoggleGameMode mode = equation ? EquationGameMode.Instance : CenturyGameMode.Instance;

            return MoggleState.StartNewGame(
                    _wordList.Value,
                    mode,
                    ImmutableDictionary.CreateRange(
                        new[]
                        {
                            new KeyValuePair<string, string>(
                                EquationGameMode.Instance.Seed.Name,
                                seed
                            ),
                            new KeyValuePair<string, string>(
                                EquationGameMode.Instance.Width.Name,
                                width.ToString()
                            ),
                            new KeyValuePair<string, string>(
                                EquationGameMode.Instance.Height.Name,
                                height.ToString()
                            ),
                            new KeyValuePair<string, string>(
                                EquationGameMode.Instance.Height.Name,
                                height.ToString()
                            )
                        }
                    )


                    );
        }

        [Theory]
        [InlineData("")]
        [InlineData("Hello")]
        [InlineData("World")]
        public async Task SameSeedShouldProduceSameGame(string seed)
        {
            var board1 = CreateFromSeed(seed).Board;

            await Task.Delay(100);

            var board2 = CreateFromSeed(seed).Board;

            TestOutputHelper.WriteLine(board1.ToString());
            TestOutputHelper.WriteLine(board2.ToString());

            board1.ToString().Should().Be(board2.ToString());
        }


        [Theory]
        [InlineData("")]
        [InlineData("mark")]
        [InlineData("ruth")]
        public void Test1(string key)
        {
            var s = CreateFromSeed(key).Board;


            s.Rows.Should().Be(4);
            s.Columns.Should().Be(4);

            s.Letters.Should().HaveCount(16);

            TestOutputHelper.WriteLine(s.ToMultiLineString());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestLetterFrequencies(bool classic)
        {

            var dict = new ConcurrentDictionary<Letter, int>();

            for (var i = 0; i < 1000; i++)
            {
                var s = CreateFromSeed(i.ToString(), classic);


                for (var j = 0; j < 16; j++)
                {
                    var l = s.Board.GetLetterAtIndex(j);
                    dict.AddOrUpdate(l, 1,(_, q) => q + 1);
                }
            }


            dict.Count.Should().Be(26);

            foreach (var (key, value) in dict.OrderBy(x=>x.Key.WordText))
            {
                TestOutputHelper.WriteLine($"{key.WordText}: {value}");
            }
        }

        [Theory]
        [InlineData(@"fxxxxoxxxxrxxxxt", 4, "fort","0,0 1,1 2,2 3,3" )]
        public void TestFindWord(string gridLetters, int width, string word,string expectedCoordinatesString)
        {
            var board = new MoggleBoard(
                gridLetters.EnumerateRunes().Select(Letter.Create).ToImmutableArray(),
                width
            );

            var letters = board.TryFindWord(word);

            letters.Should().NotBeNull();

            var expectedCoordinates = expectedCoordinatesString.Split(' ')
                .Select(Coordinate.TryParse)
                .ToList();

            letters.Should().BeEquivalentTo(expectedCoordinates);


        }

        [Theory]
        [InlineData(1000, false, 4, 4)]
        public void FindBestSeeds(int numberToGet, bool classic, int width, int height)
        {
            var seeds = new List<(string, int)>();
            var sw = Stopwatch.StartNew();
            foreach (var seed in _wordList.Value.LegalWords)
            {
                var state = CreateFromSeed(seed, classic, width, height);

                var words = state.Solver.GetPossibleSolutions(state.Board).ToList();

                seeds.Add((seed, words.Count));
            }
            sw.Stop();

            TestOutputHelper.WriteLine(sw.ElapsedMilliseconds + "ms");
            TestOutputHelper.WriteLine(seeds.Max(x=>x.Item2) + "Words");

            foreach (var p in seeds.OrderByDescending(x=>x.Item2).Take(numberToGet))
            {
                TestOutputHelper.WriteLine(p.Item1);
            }
        }


        [Fact]
        public void TestGoodSeeds() => GoodSeedHelper.GoodSeeds.Value.Count.Should().Be(1000);

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
            var state = CreateFromSeed(seed, classic, width, height);

            var sw = Stopwatch.StartNew();
            var words = state.Solver.GetPossibleSolutions(state.Board).ToList();
            sw.Stop();

            TestOutputHelper.WriteLine(sw.ElapsedMilliseconds + "ms");
            TestOutputHelper.WriteLine(words.Count + "Words");

            TestOutputHelper.WriteLine("");
            TestOutputHelper.WriteLine("");

            words.Should().NotBeEmpty();

            foreach (var word in words)
                TestOutputHelper.WriteLine(word.Display);

        }

        [Theory]
        [InlineData("abc", false, 3, 3)]
        [InlineData("abhors", false, 3, 3)]
        [InlineData("proxys", false, 3, 3)]
        [InlineData("equation", false, 3, 3)]
        [InlineData("caricaturist", false, 3, 3)]
        [InlineData("gaiety", false, 3, 3)]
        public void TestMathSolver(string seed, bool equation, int height, int width)
        {
            var state = CreateMathStateFromSeed(seed, equation, width, height);

            TestOutputHelper.WriteLine(state.Board.ToMultiLineString());

            var sw = Stopwatch.StartNew();
            var words = state.Solver.GetPossibleSolutions(state.Board).ToList();
            sw.Stop();

            TestOutputHelper.WriteLine(sw.ElapsedMilliseconds + "ms");
            TestOutputHelper.WriteLine(words.Count + "Words");

            TestOutputHelper.WriteLine("");
            TestOutputHelper.WriteLine("");

            words.Should().NotBeEmpty();

            foreach (var word in words)
                TestOutputHelper.WriteLine(word.Display);
        }

        [Theory]
        [InlineData(1000, false, 3,3)]
        [InlineData(1000, true, 3,3)]
        [InlineData(1000, false, 4,4)]
        [InlineData(1000, true, 4,4)]
        public void FindBestGrids(int trials, bool classic, int height, int width)
        {
            var bestSeed  = 0;
            var bestScore = 0;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < trials; i++)
            {
                var state = CreateFromSeed(i.ToString(), classic, width, height);
                var words = state.Solver.GetPossibleSolutions(state.Board).ToList();
                var score = words.Sum(x=>x.Points);

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
                var state = CreateFromSeed(i.ToString(), classic, width, height);
                var words = state.Solver.GetPossibleSolutions(state.Board).ToList();
                totalWords += words.Count;
                totalScore += words.Sum(x=>x.Points);
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
            foreach (var word in _wordList.Value.LegalWords.OrderBy(x=>x))
            {
                TestOutputHelper.WriteLine(word);
            }
        }

        [Theory]
        [InlineData(100)]
        public void TestCenturyAnimate(int take)
        {
            var c = CenturyGameMode.Instance.GetAnimation(
                ImmutableDictionary<string, string>.Empty.Add(CenturyGameMode.Instance.AnimateSetting.Name, true.ToString()),
                WordList.LazyInstance
            );
            var (board, _) = CenturyGameMode.Instance.CreateGame(ImmutableDictionary<string, string>.Empty, WordList.LazyInstance);

            foreach (var move in c.Steps.OfType<Step.Move>().Take(take))
            {
                var letter = board.GetLetterAtCoordinate(move.Coordinate);

                TestOutputHelper.WriteLine(letter.ButtonText);
            }

        }


    }
}
