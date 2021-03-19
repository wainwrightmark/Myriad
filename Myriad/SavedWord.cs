using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;

namespace Myriad
{

public class SavedChallengeGame
{
    public string boardId { get; set; }

    public int maxSolutions { get; set; }

    public int foundSolutions { get; set; }

    public bool IsComplete() => foundSolutions >= maxSolutions;

    public bool AreEqual(SavedChallengeGame savedChallengeGame)
    {
        return boardId.Equals(savedChallengeGame.boardId) &&
               maxSolutions.Equals(savedChallengeGame.maxSolutions) &&
               foundSolutions.Equals(savedChallengeGame.foundSolutions);
    }
}

public class SavedWord
{
    public int uniqueId { get; set; }
    public string boardId { get; set; }

    public string wordText { get; set; }

    public string coordinateString { get; set; }

    public static string CreateCoordinatesString(IEnumerable<Coordinate> path)
    {
        return path.Select(x => $"{x.Column},{x.Row}").ToDelimitedString(" ");
    }

    private static readonly Regex CoordinateRegex = new(
        @"\b(?<col>\d+),(?<row>\d+)\b",
        RegexOptions.Compiled
    );

    public IEnumerable<Coordinate> GetCoordinates()
    {
        foreach (Match match in CoordinateRegex.Matches(coordinateString))
        {
            var coordinate = new Coordinate(
                int.Parse(match.Groups["row"].Value),
                int.Parse(match.Groups["col"].Value)
            );

            yield return coordinate;
        }
    }
}

}
