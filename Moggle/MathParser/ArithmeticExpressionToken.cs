using Superpower.Display;

namespace Moggle.MathParser
{

public enum ArithmeticExpressionToken
{
    None,

    Number,

    RomanNumeral,

    [Token(Category = "operator", Example = "+")]
    Plus,

    [Token(Category = "operator", Example = "-")]
    Minus,

    [Token(Category = "operator", Example = "*")]
    Times,

    [Token(Category = "operator", Example = "/")]
    Divide,

    [Token(Category = "operator", Example = "^")]
    Power,

    [Token(Category = "operator", Example = "=")]
    Equals,

    [Token(Example = "(")] LParen,

    [Token(Example = ")")] RParen
}

}
