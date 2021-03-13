using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Divergic.Logging.Xunit;
using FluentAssertions;
using MoreLinq;
using Myriad.Creator;
using Xunit;
using Xunit.Abstractions;

namespace Myriad.Tests
{

public class CreatorTests
{
    public ITestOutputHelper TestOutputHelper { get; }

    public CreatorTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

    public static TheoryData<string,int, string, int, int> GetWordLists()
    {
        //const int delay    = 2000_000;
        const int width    = 5;
        const int height   = 5;
        const int maxTries = 3;

        var resourceSet =
            Lists.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, true, true);

        var td = new TheoryData<string,int, string, int, int>();

        foreach (DictionaryEntry entry in resourceSet!)
        {
            //td.Add(entry.Key.ToString()!, maxTries, entry.Value?.ToString()!, width, height);
            td.Add(entry.Key.ToString()!, maxTries, entry.Value?.ToString()!, width -1, height - 1);
        }

        return td;
    }

    [Theory]
    [MemberData(nameof(GetWordLists))]
    //[InlineData("Colors","red\r\ngreen\r\nblue", 10000, 3,3)]
    public void TestMostWords(
        string group,
        int maxTries,
        string wordsString,
        int width,
        int height)
    {
        var maxCoordinate = new Coordinate(height - 1, width - 1);
        var sw            = Stopwatch.StartNew();
        var logger        = new TestOutputLogger("Test", TestOutputHelper);
        var words         = GetAllWords(wordsString).ToImmutableList();

        static IEnumerable<string> GetAllWords(string text)
        {
            var words = text.ToUpper()
                .Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.None | StringSplitOptions.RemoveEmptyEntries
                )
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(LettersOnly)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return words;

            static string LettersOnly(string s) => new(s.Where(char.IsLetter).ToArray());
        }

        var ct = new CancellationTokenSource();

        var grid = GridCreator.CreateGridForMostWords(
            words,
            maxTries,
            logger,
            sw,
            maxCoordinate,
            ct.Token
        );

        grid.Should().NotBeNull();

        TestOutputHelper.WriteLine(sw.Elapsed.ToString());

        var gridText = grid.Value.grid.ToBoard(() => new Rune('_'))
            .Letters.Select(x => x.WordText.First())
            .ToDelimitedString("");

        TestOutputHelper.WriteLine(
            $"{group}; {gridText};{grid!.Value.words.ToDelimitedString(", ")}"
        );
    }

    [Theory]
    [InlineData("happy birthday")]
    [InlineData("happy birthday stephanie")]
    [InlineData("happy,birthday stephanie daybirthhappy happydaybirth")]
    [InlineData("happy birthday mark")]
    [InlineData("happy birthday hap")]
    [InlineData("a b c")]
    [InlineData("xa xb xc")]
    [InlineData("happy birthday steph")]
    [InlineData("ovington skware massive")]
    [InlineData("stacys mum has got it going on")]
    [InlineData("Amy alex emily lee mark stephanie mike lizanne amelia")]
    //[InlineData(StacysMomChorus)]
    //[InlineData(TotalEclipseChorus)]
    //[InlineData(TotalEclipse)]
    //[InlineData(StacysMum)]
    public void TestCreator(string wordsString)
    {
        const int msDelay = 10000;
        var       logger  = new TestOutputLogger("Test", TestOutputHelper);

        var grid = GridCreator.CreateNodeGridFromText(wordsString, logger, msDelay);

        TestOutputHelper.WriteLine(grid.ToBoard(() => new Rune('*')).ToMultiLineString());

        var allWords = GridCreator.GetAllWords(wordsString).ToList();

        var solver = new Solver(WordList.FromWords(allWords), new SolveSettings(2, false, null));

        var possibleWords = solver.GetPossibleSolutions(grid.ToBoard(() => new Rune('*')));

        possibleWords.Should().BeEquivalentTo(allWords.Where(x => x.Length > 1));
    }

    public const string StacysMomChorus = @"
Stacy's mom has got it goin' on
She's all I want
And I've waited for so long
Stacy, can't you see?
You're just not the girl for me
I know it might be wrong but
I'm in love with Stacy's mom
";

    public const string TotalEclipseChorus = @"
I really need you tonight
Forever's gonna start tonight
Forever's gonna start tonight
Once upon a time I was falling in love
But now I'm only falling apart
Nothing I can say
A total eclipse of the heart
A total eclipse of the heart
A total eclipse of the heart
Turn around, bright eyes";

    public const string TotalEclipse = @"every now and then I get a little bit lonely
And you're never coming 'round
(Turn around) every now and then I get a little bit tired
Of listening to the sound of my tears
(Turn around) every now and then I get a little bit nervous
That the best of all the years have gone by
(Turn around) every now and then I get a little bit terrified
And then I see the look in your eyes
(Turn around, bright eyes) every now and then I fall apart
(Turn around, bright eyes) every now and then I fall apart
And I need you now tonight
And I need you more than ever
And if you only hold me tight
We'll be holding on forever
And we'll only be making it right
'Cause we'll never be wrong
Together we can take it to the end of the line
Your love is like a shadow on me all of the time (all of the time)
I don't know what to do and I'm always in the dark
We're living in a powder keg and giving off sparks
I really need you tonight
Forever's gonna start tonight
Forever's gonna start tonight
Once upon a time I was falling in love
But now I'm only falling apart
There's nothing I can do
A total eclipse of the heart
Once upon a time there was light in my life
But now there's only love in the dark
Nothing I can say
A total eclipse of the heart
every now and then I fall apart
(Turn around, bright eyes) every now and then I fall apart
And I need you now tonight (and I need you now)
And I need you more than ever
And if you only hold me tight (if you only)
We'll be holding on forever
And we'll only be making it right (and we'll never)
'Cause we'll never be wrong
Together we can take it to the end of the line
Your love is like a shadow on me all of the time (all of the time)
I don't know what to do, I'm always in the dark
We're living in a powder keg and giving off sparks
I really need you tonight
Forever's gonna start tonight
Forever's gonna start tonight
Once upon a time I was falling in love
But now I'm only falling apart
Nothing I can say
A total eclipse of the heart
A total eclipse of the heart
A total eclipse of the heart
Turn around, bright eyes";

    public const string StacysMum =
        @"Stacy's mom has got it goin' on
Stacy's mom has got it goin' on
Stacy's mom has got it goin' on
Stacy's mom has got it goin' on
Stacy, can I come over after school?
(After school)
We can hang around by the pool
(Hang by the pool)
Did your mom get back from her business trip?
(Business trip)
Is she there, or is she trying to give me the slip?
(Give me the slip)
You know, I'm not the little boy that I used to be
I'm all grown up now
Baby, can't you see?
Stacy's mom has got it goin' on
She's all I want
And I've waited for so long
Stacy, can't you see?
You're just not the girl for me
I know it might be wrong but
I'm in love with Stacy's mom
Stacy's mom has got it goin' on
Stacy's mom has got it goin' on
Stacy, do you remember when I mowed your lawn?
(Mowed your lawn)
Your mom came out with just a towel on
(Towel on)
I could tell she liked me from the way she stared
(The way she stared)
And the way she said
""You missed a spot over there""
(A spot over there)
And I know that you think it's just a fantasy
But since your dad walked out
Your mom could use a guy like me
Stacy's mom has got it goin' on
She's all I want
And I've waited for so long
Stacy, can't you see?
You're just not the girl for me
I know it might be wrong but
I'm in love with Stacy's mom
Stacy's mom has got it goin' on (she's got it going on)
She's all I want and I've waited for so long (waited and waited)
Stacy, can't you see?
You're just not the girl for me
I know it might be wrong
I'm in love with
Stacy's mom, oh, oh
(I'm in love with)
Stacy's mom, oh, oh
(Wait a minute)
Stacy, can't you see?
You're just not the girl for me
I know it might be wrong but
I'm in love with Stacy's mom";
}

}
