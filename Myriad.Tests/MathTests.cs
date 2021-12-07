using FluentAssertions;
using Myriad.MathParser;
using Xunit;
using Xunit.Abstractions;

namespace Myriad.Tests;

public class MathTest
{
    public ITestOutputHelper TestOutputHelper { get; }

    public MathTest(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData("",        false)]
    [InlineData("1=1",     true)]
    [InlineData("1+2=3",   true)]
    [InlineData("1+2=4",   false)]
    [InlineData("3+2=4+1", true)]
    [InlineData("3*2=4+2", true)]
    [InlineData("3*2",     false)]
    [InlineData("blah",    false)]
    [InlineData("2=3*",    false)]
    public void ShouldParseEquation(string text, bool expectedResult)
    {
        var r = Parser.IsValidEquation(text);

        r.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("",       null)]
    [InlineData("0",      0)]
    [InlineData("-1",     -1)]
    [InlineData("--1",    1)]
    [InlineData("2+--1",  3)]
    [InlineData("---1",   -1)]
    [InlineData("12",     12)]
    [InlineData("1+2",    3)]
    [InlineData("11+12",  23)]
    [InlineData("11*12",  132)]
    [InlineData("11-12",  -1)]
    [InlineData("1+*-12", null)]
    [InlineData("1+2=3",  null)]
    [InlineData("4/2",    2)]
    [InlineData("4/3",    null)]
    [InlineData("4+-3",   1)]
    [InlineData("4-+3",   null)]
    [InlineData("3*-4",   -12)]
    [InlineData("2^5",    32)]
    [InlineData("3^3",    27)]
    [InlineData("3!",     6)]
    [InlineData("3!!",    720)]
    [InlineData("!3",     2)]
    [InlineData("!3 + 5", 7)]
    [InlineData("3! + 5", 11)]
    [InlineData("III!",   6)]
    [InlineData("III!!",  720)]
    [InlineData("!III!",  265)]
    public void ShouldParseExpression(string text, int? expectedResult)
    {
        var r = Parser.GetExpressionValue(text);

        r.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("i",         1)]
    [InlineData("xi",        11)]
    [InlineData("xiiii",     null)]
    [InlineData("ix",        9)]
    [InlineData("q",         null)]
    [InlineData("MCMXCVIII", 1998)]
    [InlineData("mcmxcviii", 1998)]
    [InlineData("MMXVIII",   2018)]
    [InlineData("mcmc",      null)]
    public void ShouldParseRomanNumeral(string text, int? expectedResult)
    {
        var r = RomanNumeralParser.ConvertRomanToNumber(text);

        r.Should().Be(expectedResult);

        var r2 = Parser.GetExpressionValue(text);

        r2.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("i",         1)]
    [InlineData("i + i",     2)]
    [InlineData("i * i",     1)]
    [InlineData("i - i",     0)]
    [InlineData("vi / ii",   3)]
    [InlineData("xi",        11)]
    [InlineData("xiiii",     null)]
    [InlineData("ix",        9)]
    [InlineData("q",         null)]
    [InlineData("MCMXCVIII", 1998)]
    [InlineData("mcmxcviii", 1998)]
    [InlineData("MMXVIII",   2018)]
    [InlineData("mcmc",      null)]
    public void ShouldParseRomanNumeralExpression(string text, int? expectedResult)
    {
        var r2 = Parser.GetExpressionValue(text);

        r2.Should().Be(expectedResult);
    }

}