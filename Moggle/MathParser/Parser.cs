using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Moggle.MathParser
{

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
        var r = _expressionValuesCache.GetOrAdd(pattern, Calculate);
        return r;

        static int? Calculate(string pattern)
        {
            var tokenListResult = Tokenizer.TryTokenize(pattern);

            if (!tokenListResult.HasValue)
                return null;

            var parseResult = Lambda.TryParse(tokenListResult.Value);

            if (!parseResult.HasValue)
                return null;

            decimal value;

            try
            {
                value = parseResult.Value.Compile().Invoke();
            }
            catch (Exception)
            {
                return null;
            }

            var i = Convert.ToInt32(value);

            if (value == i)
                return i;

            return null;//The number was not an integer - it may have been e.g. 0.5
        }
    }

    private static readonly ConcurrentDictionary<string, int?> _expressionValuesCache =
        new();

    public static readonly Tokenizer<ArithmeticExpressionToken> Tokenizer =
        new TokenizerBuilder<ArithmeticExpressionToken>()
            .Ignore(Span.WhiteSpace)
            .Match(Character.EqualTo('+'), ArithmeticExpressionToken.Plus)
            .Match(Character.EqualTo('-'), ArithmeticExpressionToken.Minus)
            .Match(Character.EqualTo('*'), ArithmeticExpressionToken.Times)
            .Match(Character.EqualTo('/'), ArithmeticExpressionToken.Divide)
            .Match(Character.EqualTo('='), ArithmeticExpressionToken.Equals)
            .Match(Character.EqualTo('('), ArithmeticExpressionToken.LParen)
            .Match(Character.EqualTo(')'), ArithmeticExpressionToken.RParen)
            .Match(Numerics.Natural,       ArithmeticExpressionToken.Number)
            .Build();

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Add =
        Token.EqualTo(ArithmeticExpressionToken.Plus).Value(ExpressionType.AddChecked);

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Subtract =
        Token.EqualTo(ArithmeticExpressionToken.Minus).Value(ExpressionType.SubtractChecked);

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Multiply =
        Token.EqualTo(ArithmeticExpressionToken.Times).Value(ExpressionType.MultiplyChecked);

    static readonly TokenListParser<ArithmeticExpressionToken, ExpressionType> Divide =
        Token.EqualTo(ArithmeticExpressionToken.Divide).Value(ExpressionType.Divide);

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Constant =
        Token.EqualTo(ArithmeticExpressionToken.Number)
            .Apply(Numerics.DecimalDecimal)
            .Select(n => (Expression)Expression.Constant(n));

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Factor =
        (from lparen in Token.EqualTo(ArithmeticExpressionToken.LParen)
         from expr in Parse.Ref(() => Expr)
         from rparen in Token.EqualTo(ArithmeticExpressionToken.RParen)
         select expr)
        .Or(Constant);

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Operand =
        (from sign in Token.EqualTo(ArithmeticExpressionToken.Minus)
         from factor in Factor
         select (Expression)Expression.Negate(factor))
        .Or(Factor)
        .Named("expression");

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Term =
        Parse.Chain(Multiply.Or(Divide), Operand, Expression.MakeBinary);

    static readonly TokenListParser<ArithmeticExpressionToken, Expression> Expr =
        Parse.Chain(Add.Or(Subtract), Term, Expression.MakeBinary);

    public static readonly TokenListParser<ArithmeticExpressionToken, Expression<Func<decimal>>> Lambda
        =
        Expr.AtEnd().Select(body => Expression.Lambda<Func<decimal>>(body));

    public static readonly TokenListParser<ArithmeticExpressionToken, Equation> Equation =
        (from l in Expr
         from eq in Token.EqualTo(ArithmeticExpressionToken.Equals)
         from r in Expr.AtEnd()
         select new Equation(l, r)
        );
}

}
