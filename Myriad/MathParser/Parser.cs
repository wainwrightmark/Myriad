using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Myriad.MathParser
{

public static class RomanNumeralParser
{
    private static readonly Lazy<Dictionary<string, int>> RomanDictionary =
        new(
            () => Enumerable.Range(1, 3999)
                .ToDictionary(x => ToRoman(x)!, x => x, StringComparer.OrdinalIgnoreCase)
        );

    public static string? ToRoman(int number)
    {
        return number switch
        {
            < 0     => null,
            > 3999  => null,
            < 1     => string.Empty,
            >= 1000 => "M" + ToRoman(number - 1000),
            >= 900  => "CM" + ToRoman(number - 900),
            >= 500  => "D" + ToRoman(number - 500),
            >= 400  => "CD" + ToRoman(number - 400),
            >= 100  => "C" + ToRoman(number - 100),
            >= 90   => "XC" + ToRoman(number - 90),
            >= 50   => "L" + ToRoman(number - 50),
            >= 40   => "XL" + ToRoman(number - 40),
            >= 10   => "X" + ToRoman(number - 10),
            >= 9    => "IX" + ToRoman(number - 9),
            >= 5    => "V" + ToRoman(number - 5),
            >= 4    => "IV" + ToRoman(number - 4),
            >= 1    => "I" + ToRoman(number - 1)
        };
    }

    public static int? ConvertRomanToNumber(string text)
    {
        if (RomanDictionary.Value.TryGetValue(text, out var v))
            return v;

        return null;
    }

    /// <summary>
    /// A string of digits, converted into a <see cref="T:System.UInt32" />.
    /// </summary>
    public static TextParser<int> TextParser { get; } = input =>
    {
        var v = ConvertRomanToNumber(input.ToStringValue());

        if (v is null)
            return Result.Empty<int>(input, "Expected roman numeral");

        return Result.Value(v.Value, input, TextSpan.Empty);
    };
}

public static class Parser
{
    public static bool IsValidEquation(string pattern)
    {
        var tokenListResult = Tokenizer.TryTokenize(pattern);

        if (!tokenListResult.HasValue)
            return false;

        var parseResult = Equation.TryParse(tokenListResult.Value);

        if (!parseResult.HasValue)
            return false;

        var isValid = parseResult.Value.IsValid;

        return isValid;
    }

    public static int? GetExpressionValue(string pattern)
    {
        var r = ExpressionValuesCache.GetOrAdd(pattern, Calculate);
        return r;

        static int? Calculate(string pattern)
        {
            var tokenListResult = Tokenizer.TryTokenize(pattern);

            if (!tokenListResult.HasValue)
                return null;

            var parseResult = Lambda.TryParse(tokenListResult.Value);

            if (!parseResult.HasValue)
                return null;

            return parseResult.Value;
        }
    }

    private static readonly ConcurrentDictionary<string, int?> ExpressionValuesCache =
        new();

    public static readonly HashSet<char> RomanNumerals = new()
    {
        'i',
        'I',
        'v',
        'V',
        'x',
        'X',
        'l',
        'L',
        'c',
        'C',
        'd',
        'D',
        'm',
        'M'
    };

    public static readonly Tokenizer<ArithmeticExpressionToken> Tokenizer =
        new TokenizerBuilder<ArithmeticExpressionToken>()
            .Ignore(Span.WhiteSpace)
            .Match(Character.EqualTo('+'),               ArithmeticExpressionToken.Plus)
            .Match(Character.EqualTo('-'),               ArithmeticExpressionToken.Minus)
            .Match(Character.EqualTo('*'),               ArithmeticExpressionToken.Times)
            .Match(Character.EqualTo('/'),               ArithmeticExpressionToken.Divide)
            .Match(Character.EqualTo('^'),               ArithmeticExpressionToken.Power)
            .Match(Character.EqualTo('='),               ArithmeticExpressionToken.Equals)
            .Match(Character.EqualTo('('),               ArithmeticExpressionToken.LParen)
            .Match(Character.EqualTo(')'),               ArithmeticExpressionToken.RParen)
            .Match(Character.EqualTo('!'),               ArithmeticExpressionToken.Bang)
            .Match(Span.WithAll(RomanNumerals.Contains), ArithmeticExpressionToken.RomanNumeral)
            .Match(Numerics.Natural,                     ArithmeticExpressionToken.Number)
            .Build();

    enum InfixExpressionType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power
    }

    static readonly TokenListParser<ArithmeticExpressionToken, InfixExpressionType> Add =
        Token.EqualTo(ArithmeticExpressionToken.Plus).Value(InfixExpressionType.Add);

    static readonly TokenListParser<ArithmeticExpressionToken, InfixExpressionType> Subtract =
        Token.EqualTo(ArithmeticExpressionToken.Minus).Value(InfixExpressionType.Subtract);

    static readonly TokenListParser<ArithmeticExpressionToken, InfixExpressionType> Multiply =
        Token.EqualTo(ArithmeticExpressionToken.Times).Value(InfixExpressionType.Multiply);

    static readonly TokenListParser<ArithmeticExpressionToken, InfixExpressionType> Divide =
        Token.EqualTo(ArithmeticExpressionToken.Divide).Value(InfixExpressionType.Divide);

    static readonly TokenListParser<ArithmeticExpressionToken, InfixExpressionType> Power =
        Token.EqualTo(ArithmeticExpressionToken.Power).Value(InfixExpressionType.Power);

    static readonly TokenListParser<ArithmeticExpressionToken, int?> Constant =
        Token.EqualTo(ArithmeticExpressionToken.Number)
            .Apply(Numerics.IntegerInt32)
            .Select(n => n as int?);

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> RomanNumeral =
        Token.EqualTo(ArithmeticExpressionToken.RomanNumeral)
            .Apply(RomanNumeralParser.TextParser)
            .Select(x => x as int?);

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> L1 =
        Constant.Or(RomanNumeral);

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> Bracketed =
        (from leftParenthesis in Token.EqualTo(ArithmeticExpressionToken.LParen)
         from expr in Parse.Ref(() => Expr)
         from rightParenthesis in Token.EqualTo(ArithmeticExpressionToken.RParen)
         select expr);

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> L2 = Bracketed.Or(L1);

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> FactorialMaybe =
        (from num in L2
         from b in Token.EqualTo(ArithmeticExpressionToken.Bang).Many()
         select (num, b)
        )
        .Where(x => x.num.HasValue)
        .Select(
            x =>
            {
                var r     = x.num;
                var times = x.b.Length;

                while (r.HasValue && times > 0)
                {
                    r = BangFunctions.Factorial(r.Value);
                    times--;
                }

                return r;
            }
        );

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> L3 = FactorialMaybe;

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> SubFactorial =
        (from b in Token.EqualTo(ArithmeticExpressionToken.Bang)
         from num in L3
         select num
        )
        .Where(x => x.HasValue)
        .Select(x => BangFunctions.SubFactorial(x!.Value));

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> L4 =
        SubFactorial.Or(L3);

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> Negative =
        (from sign in Token.EqualTo(ArithmeticExpressionToken.Minus).AtLeastOnce()
         from factor in L4
         select sign.Length % 2 == 0? factor : -factor);

    private static readonly TokenListParser<ArithmeticExpressionToken, int?> L5 = Negative.Or(L4);

    static readonly TokenListParser<ArithmeticExpressionToken, int?> Term = Parse.Chain(
        Multiply.Or(Divide).Or(Power),
        L5,
        ApplyInfix
    );

    static readonly TokenListParser<ArithmeticExpressionToken, int?> Expr = Parse.Chain(
        Add.Or(Subtract),
        Term,
        ApplyInfix
    );

    public static readonly TokenListParser<ArithmeticExpressionToken, int?>
        Lambda
            =
            Expr.AtEnd().Select(body => body);

    public static readonly TokenListParser<ArithmeticExpressionToken, Equation> Equation =
        (from l in Expr
         from eq in Token.EqualTo(ArithmeticExpressionToken.Equals)
         from r in Expr.AtEnd()
         select new Equation(l, r)
        );

    private static int? ApplyInfix(InfixExpressionType expressionType, int? i1, int? i2)
    {
        if (!i1.HasValue || !i2.HasValue)
            return null;

        switch (expressionType)
        {
            case InfixExpressionType.Add:      return i1 + i2;
            case InfixExpressionType.Subtract: return i1 - i2;
            case InfixExpressionType.Multiply: return i1 * i2;
            case InfixExpressionType.Divide:
            {
                if (i2.Value == 0)
                    return null;

                var div = Math.DivRem(i1.Value, i2.Value, out var rem);
                return rem == 0 ? div : null;
            }
            case InfixExpressionType.Power:
            {
                var d = Math.Pow(i1.Value, i2.Value);

                try
                {
                    var i = Convert.ToInt32(d);
                    return i;
                }
                catch (OverflowException)
                {
                    return null;
                }
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(expressionType), expressionType, null);
        }
    }
}

public static class BangFunctions
{
    public static int? Factorial(int x) => Factorials.TryGetValue(x, out var v) ? v : null;
    public static int? SubFactorial(int x) => SubFactorials.TryGetValue(x, out var v) ? v : null;

    private static readonly Dictionary<int, int> Factorials = Enumerable.Range(-20, 40)
        .ToDictionary(x => x, Fact);

    private static readonly Dictionary<int, int> SubFactorials = new()
    {
        { 1, 0 },
        { 2, 1 },
        { 3, 2 },
        { 4, 9 },
        { 5, 44 },
        { 6, 265 },
        { 7, 1854 },
        { 8, 14833 },
        { 9, 133496 },
        { 10, 1334961 },
    };

    static int Fact(int i)
    {
        if (i < 0)
            return -1 * Fact(-i);

        if (i == 0 || i == 1)
            return 1;

        var sub = Fact(i - 1);
        return i * sub;
    }
}

}
