using System;
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
        [InlineData("mark")]
        [InlineData("ruth")]
        public void Test1(string key)
        {
            var s = MoggleBoard.DefaultBoardClassic;

            s = s.Randomize(key);

            s.RowCount.Should().Be(4);
            s.ColumnCount.Should().Be(4);

            s.Positions.Should().HaveCount(16);
            s.Dice.Should().HaveCount(16);

            for (var r = 0; r < s.RowCount; r++)
            {
                for (var c = 0; c < s.ColumnCount; c++)
                {
                    var val = s.GetLetterAtCoordinate(new Coordinate(r, c));

                    TestOutputHelper.WriteLine($"{r}, {c}: {val}");
                }
            }
        }
    }
}
