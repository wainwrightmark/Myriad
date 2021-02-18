using System;
using System.Linq.Expressions;
using Superpower;
using Superpower.Display;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace Moggle
{

    public enum ArithmeticExpressionToken
    {
        None,

        Number,

        [Token(Category = "operator", Example = "+")]
        Plus,

        [Token(Category = "operator", Example = "-")]
        Minus,

        [Token(Category = "operator", Example = "*")]
        Times,

        [Token(Category = "operator", Example = "-")]
        Divide,

        [Token(Category = "operator", Example = "=")]
        Equals,

        [Token(Example = "(")] LParen,

        [Token(Example = ")")] RParen
    }


public record Equation(Expression Left, Expression Right)
{
    public bool IsValid
    {
        get
        {


            var l = Expression.Lambda<Func<int>>(Left).Compile().Invoke();
            var r = Expression.Lambda<Func<int>>(Right).Compile().Invoke();

            return l == r;
        }
    }
}

public static class MathParser
{
    public static bool IsValidEquation(string s)
    {
        var r = tokenizer.TryTokenize(s);

        if (!r.HasValue)
            return false;

        var pv = Equation.TryParse(r.Value);

        if (!pv.HasValue)
            return false;

        var isValid = pv.Value.IsValid;

        return isValid;
    }

    public static readonly Tokenizer<ArithmeticExpressionToken> tokenizer =
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
            .Apply(Numerics.IntegerInt32)
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

    public static readonly TokenListParser<ArithmeticExpressionToken, Expression<Func<int>>> Lambda
        =
        Expr.AtEnd().Select(body => Expression.Lambda<Func<int>>(body));

    public static readonly TokenListParser<ArithmeticExpressionToken, Equation> Equation =
        (from l in Expr
         from eq in Token.EqualTo(ArithmeticExpressionToken.Equals)
         from r in Expr.AtEnd()
         select new Equation(l, r)
        );
}

}
