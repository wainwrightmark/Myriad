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
    }
}
