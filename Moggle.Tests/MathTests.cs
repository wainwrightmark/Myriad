using FluentAssertions;
using Moggle.MathParser;
using Xunit;
using Xunit.Abstractions;

namespace Moggle.Tests
{
    public class MathTest
    {
        public ITestOutputHelper TestOutputHelper { get; }

        public MathTest(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("1=1",true)]
        [InlineData("1+2=3", true)]
        [InlineData("1+2=4", false)]
        [InlineData("3+2=4+1", true)]
        [InlineData("3*2=4+2", true)]
        [InlineData("3*2", false)]
        [InlineData("blah", false)]
        [InlineData("2=3*", false)]
        public void ShouldParseEquation(string text, bool expectedResult)
        {
            var r= Parser.IsValidEquation(text);

            r.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("0", 0)]
        [InlineData("12", 12)]
        [InlineData("1+2", 3)]
        [InlineData("11+12", 23)]
        [InlineData("11*12", 132)]
        [InlineData("11-12", -1)]
        [InlineData("1+*-12", null)]
        [InlineData("1+2=3", null)]
        [InlineData("4/2", 2)]
        [InlineData("4/3", null)]
        public void ShouldParseExpression(string text, int? expectedResult)
        {
            var r = Parser.GetExpressionValue(text);

            r.Should().Be(expectedResult);
        }


    }
}
