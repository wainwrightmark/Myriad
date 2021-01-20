using System;

namespace Moggle
{

public record DicePosition(int DiceIndex, int FaceIndex)
{
    public DicePosition RandomizeFace(Random random)
    {
        var fi = random.Next(5);
        return this with { FaceIndex = fi };
    }
}

}