using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Myriad;

class ListComparer<T> : IComparer<ImmutableList<T>> where T : IComparable
{
    private ListComparer() { }
    public static IComparer<ImmutableList<T>> Instance { get; } = new ListComparer<T>();

    /// <inheritdoc />
    public int Compare(ImmutableList<T>? x, ImmutableList<T>? y)
    {
        if (x is null)
            return y is null? 0 : -1;

        if (y is null)
            return 1;

        var min = Math.Min(x.Count, y.Count);

        for (var i = 0; i < min; i++)
        {
            var c = x[i].CompareTo(y[i]);

            if (c != 0)
                return c;
        }

        return x.Count.CompareTo(y.Count);
    }
}