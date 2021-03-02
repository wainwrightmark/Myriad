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
        var (actualRow, actualColumn) = new Coordinate(row, column).RotateAndFlip(new Coordinate(size - 1, size - 1), rotation, false);

        actualRow.Should().Be(expectedRow);
        actualColumn.Should().Be(expectedColumn);
    }

    [Theory]
    [InlineData(2,2)]
    [InlineData(3,3)]
    [InlineData(4,4)]
    [InlineData(5,5)]
    [InlineData(6,6)]
    [InlineData(4,6)]
    [InlineData(6,4)]
    [InlineData(2,3)]
    [InlineData(3,2)]
    public void RotationShouldRotateAllPointsCorrectly(int height, int width)
    {
        var maxCoordinate = new Coordinate(height - 1, width - 1);

        for (var row = 0; row < height; row++)
        {
            for (var column = 0; column < width; column++)
            {
                var rotate0 =new Coordinate(row, column);
                var rotate1 = Rotate(rotate0, 1);
                var rotate2 = Rotate(rotate0, 2);
                var rotate3 = Rotate(rotate0, 3);

                TestOutputHelper.WriteLine($"Testing r0:{rotate0}, r1{rotate1}, r2:{rotate2}, r3{rotate3}");


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

        Coordinate Rotate(Coordinate coordinate, int rotation)
        {
            return coordinate.RotateAndFlip(maxCoordinate, rotation, false);
        }
    }
}

}
