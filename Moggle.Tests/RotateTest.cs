using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Moggle.Tests
{

public class RotateTest
{
    public ITestOutputHelper TestOutputHelper;

    public RotateTest(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0, 4)]
    [InlineData(0, 0, 1, 3, 0, 4)]
    [InlineData(0, 0, 2, 3, 3, 4)]
    [InlineData(0, 0, 3, 0, 3, 4)]
    public void RotationShouldWorkProperly(
        int row,
        int column,
        int rotation,
        int expectedRow,
        int expectedColumn,
        int size)
    {
        var (actualRow, actualColumn) = MoggleState.RotateCoordinate(row, column, size, rotation);

        actualRow.Should().Be(expectedRow);
        actualColumn.Should().Be(expectedColumn);
    }

    [Fact]
    public void RotationShouldRotateAllPointsCorrectly()
    {
        const int size = 4;

        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                TestOutputHelper.WriteLine($"Testing ({row}, {column})");

                var rotate0 = (row, column);
                var rotate1 = Rotate(rotate0, 1);
                var rotate2 = Rotate(rotate0, 2);
                var rotate3 = Rotate(rotate0, 3);

                Rotate(rotate1, 1).Should().Be(rotate2);
                Rotate(rotate1, 2).Should().Be(rotate3);
                Rotate(rotate1, 3).Should().Be(rotate0);

                Rotate(rotate2, 1).Should().Be(rotate3);
                Rotate(rotate2, 2).Should().Be(rotate0);
                Rotate(rotate2, 3).Should().Be(rotate1);

                Rotate(rotate3, 1).Should().Be(rotate0);
                Rotate(rotate3, 2).Should().Be(rotate1);
                Rotate(rotate3, 3).Should().Be(rotate2);
            }
        }

        static (int row, int column) Rotate((int row, int column) pair, int rotation)
        {
            return MoggleState.RotateCoordinate(pair.row, pair.column, size, rotation);
        }
    }
}

}
