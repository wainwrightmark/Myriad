using FluentAssertions;
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
            var r= MathParser.IsValidEquation(text);

            r.Should().Be(expectedResult);
        }

    }
}
