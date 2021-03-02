using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Moggle.Creator
{

public record NodeGrid(
    ImmutableSortedDictionary<Coordinate, ImmutableSortedSet<Node>> Dictionary,
    Coordinate MaxCoordinate)
{
    private int? _hashCode;

    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => _hashCode ??= CalculateHashCode();

    private ImmutableSortedSet<Coordinate>? _allUnusedLocations = null;

    public ImmutableSortedSet<Coordinate> GetAllUnusedLocations() =>
        _allUnusedLocations ??= CalculateUnusedLocations();

    private ImmutableSortedSet<Coordinate> CalculateUnusedLocations()
    {
        var r = MaxCoordinate.GetPositionsUpTo().Except(Dictionary.Keys).ToImmutableSortedSet();
        return r;
    }

    private ImmutableDictionary<Node, Coordinate>? _nodeLocations;

    public ImmutableDictionary<Node, Coordinate> GetNodeLocations() =>
        _nodeLocations ??= CalculateNodeLocations();

    private ImmutableDictionary<Node, Coordinate> CalculateNodeLocations() => Dictionary
        .SelectMany(kvp => kvp.Value.Select(v => (v, kvp.Key)))
        .ToImmutableDictionary(x => x.v, x => x.Key);

    private int CalculateHashCode()
    {
        var code = DictionaryPermutations.Select(GetKey).Min();

        return code + MaxCoordinate.GetHashCode();

        static int GetKey(
            IEnumerable<KeyValuePair<Coordinate, ImmutableSortedSet<Node>>> coordinates)
        {
            var c = 2;

            foreach (var kvp in coordinates)
            {
                var v = PairComparer.Instance.GetHashCode(kvp);
                c += v;
                c *= 17;
            }

            return c;
        }
    }

    private IEnumerable<IEnumerable<KeyValuePair<Coordinate, ImmutableSortedSet<Node>>>>
        DictionaryPermutations
    {
        get
        {
            for (var rotation = 0; rotation < 4; rotation++)
            {
                for (var reflection = 0; reflection < 1; reflection++)
                {
                    if (rotation == 0 && reflection == 0)
                        yield return Dictionary;
                    else
                    {
                        static Coordinate ChangeCoordinate(
                            Coordinate c,
                            int rotation,
                            int reflection,
                            Coordinate maxCoordinate)
                        {
                            return c.RotateAndFlip(maxCoordinate, rotation, reflection == 0)
                                .ReflectColumn(maxCoordinate.Column);
                        }

                        yield return Dictionary.Select(
                                kvp => new KeyValuePair<Coordinate, ImmutableSortedSet<Node>>(
                                    ChangeCoordinate(kvp.Key, rotation, reflection, MaxCoordinate),
                                    kvp.Value
                                )
                            )
                            .OrderBy(x => x.Key)
                            .ToList();
                    }
                }
            }
        }
    }

    public virtual bool Equals(NodeGrid? ng)
    {
        if (ng is null)
            return false;

        if (ReferenceEquals(this, ng))
            return true;

        if (!MaxCoordinate.Equals(ng.MaxCoordinate))
            return false;

        if (GetHashCode() != ng.GetHashCode())
            return false;

        foreach (var dp in DictionaryPermutations)
        {
            if (ng.Dictionary.SequenceEqual(dp, PairComparer.Instance))
                return true;
        }

        return false;
    }

    private record
        PairComparer : IEqualityComparer<KeyValuePair<Coordinate, ImmutableSortedSet<Node>>>
    {
        private PairComparer() { }
        public static PairComparer Instance { get; } = new();

        public bool Equals(
            KeyValuePair<Coordinate, ImmutableSortedSet<Node>> x,
            KeyValuePair<Coordinate, ImmutableSortedSet<Node>> y)
        {
            return x.Key.Equals(y.Key) && x.Value.SequenceEqual(y.Value);
        }

        public int GetHashCode(KeyValuePair<Coordinate, ImmutableSortedSet<Node>> obj) =>
            HashCode.Combine(obj.Key, obj.Value.Count, obj.Value.First().Id);
    }

    public MoggleBoard ToMoggleBoard(Func<Rune> getFillerRune)
    {
        var builder = new List<Letter>();

        foreach (var coordinate in MaxCoordinate.GetPositionsUpTo())
        {
            Rune rune;

            if (Dictionary.TryGetValue(coordinate, out var node))
                rune = node.First().Rune;
            else
                rune = getFillerRune();

            builder.Add(Letter.Create(rune));
        }

        return new MoggleBoard(builder.ToImmutableArray(), MaxCoordinate.Column + 1);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToMoggleBoard(() => new Rune('-')).ToString();
    }
}

}
