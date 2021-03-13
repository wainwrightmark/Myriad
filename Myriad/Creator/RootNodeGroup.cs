using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Myriad.Creator
{

public class RootNodeGroup
{
    public RootNodeGroup(Rune rune) => Rune = rune;

    public ImmutableList<RootNode> RootNodes { get; set; }
    public Rune Rune { get; }

    public IReadOnlyList<IReadOnlyList<ConstraintNode>> Constraints => _constraintNodes;

    private readonly List<IReadOnlyList<ConstraintNode>> _constraintNodes = new();

    public void AddConstraintNode(IReadOnlyList<ConstraintNode> constraintNodes) =>
        _constraintNodes.Add(constraintNodes);


    private double? _constraintScore;

    public double ConstraintScore => _constraintScore ??= CalculateConstraintScore();

    private double CalculateConstraintScore()
    {
        double cNodes = Constraints.SelectMany(x => x).SelectMany(x => x.AdjacentNodes).Distinct().Count();
        //TODO extra score for constraints
        var r = cNodes / RootNodes.Count;
        return r;
    }




    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Rune} {RootNodes.Count}";
    }
}

}
