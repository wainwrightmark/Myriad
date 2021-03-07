﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Moggle.MathParser
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

            double value;
            int    i;

            try
            {
                value = parseResult.Value.Compile().Invoke();
                i     = Convert.ToInt32(Math.Round(value));
            }
            catch (Exception)
            {
                return null;
            }

            if (Math.Abs(value - i) < 0.0001)
                return i;

            return null; //The number was not an integer - it may have been e.g. 0.5
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
            .Match(Span.WithAll(RomanNumerals.Contains), ArithmeticExpressionToken.RomanNumeral)
            .Match(Numerics.Natural,                     ArithmeticExpressionToken.Number)
            .Build();

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Add =
        Token.EqualTo(ArithmeticExpressionToken.Plus).Value(ExpressionType.AddChecked);

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Subtract =
        Token.EqualTo(ArithmeticExpressionToken.Minus).Value(ExpressionType.SubtractChecked);

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Multiply =
        Token.EqualTo(ArithmeticExpressionToken.Times).Value(ExpressionType.MultiplyChecked);

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Divide =
        Token.EqualTo(ArithmeticExpressionToken.Divide).Value(ExpressionType.Divide);

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Power =
        Token.EqualTo(ArithmeticExpressionToken.Power).Value(ExpressionType.Power);

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Constant =
        Token.EqualTo(ArithmeticExpressionToken.Number)
            .Apply(Numerics.DecimalDouble)
            .Select(n => (Expression)Expression.Constant(n));

    private static readonly TokenListParser<ArithmeticExpressionToken, Expression> RomanNumeral =
        Token.EqualTo(ArithmeticExpressionToken.RomanNumeral)
            .Apply(RomanNumeralParser.TextParser)
            .Select(i => (Expression)Expression.Constant(Convert.ToDouble(i)));

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Factor =
        (from lparen in Token.EqualTo(ArithmeticExpressionToken.LParen)
         from expr in Parse.Ref(() => Expr)
         from rparen in Token.EqualTo(ArithmeticExpressionToken.RParen)
         select expr)
        .Or(Constant)
        .Or(RomanNumeral);

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Operand =
        (from sign in Token.EqualTo(ArithmeticExpressionToken.Minus)
         from factor in Factor
         select (Expression)Expression.Negate(factor))
        .Or(Factor)
        .Named("expression");

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Term =
        Parse.Chain(Multiply.Or(Divide).Or(Power), Operand, Expression.MakeBinary);

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Expr =
        Parse.Chain(Add.Or(Subtract), Term, Expression.MakeBinary);

    public static readonly TokenListParser<ArithmeticExpressionToken, Expression<Func<double>>>
        Lambda
            =
            Expr.AtEnd().Select(body => Expression.Lambda<Func<double>>(body));

    public static readonly TokenListParser<ArithmeticExpressionToken, Equation> Equation =
        (from l in Expr
         from eq in Token.EqualTo(ArithmeticExpressionToken.Equals)
         from r in Expr.AtEnd()
         select new Equation(l, r)
        );
}

}