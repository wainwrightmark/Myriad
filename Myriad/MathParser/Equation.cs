using System;
using System.Linq.Expressions;

namespace Myriad.MathParser
{

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

}
