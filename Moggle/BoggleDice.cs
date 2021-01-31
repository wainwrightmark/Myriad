using System.Collections.Immutable;
using System.Text;

namespace Moggle
{

public record BoggleDice(ImmutableList<Rune> Runes)
{
    public BoggleDice(string letters) : this(letters.EnumerateRunes().ToImmutableList()){}
};

}
