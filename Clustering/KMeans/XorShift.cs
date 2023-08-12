using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Clustering.KMeans;

// https://en.wikipedia.org/wiki/Xorshift#xorshift*
// Yo i am not about to create my own random generator since
// i won't know if it is wrong or not. Instead i have copied
// a bunch of code from existing algorithms and dotnets implementation.
internal sealed class XorShift
{
    private ulong state;

    public XorShift(uint seed)
    {
        state = seed;
    }

    public uint Next()
    {
        return (uint)(NextUInt64() >> 32);
    }

    private ulong NextUInt64()
    {
        state ^= state >> 12;
        state ^= state << 25;
        state ^= state >> 27;
        return state * 0x2545F4914F6CDD1DUL;
    }

    // https://github.com/dotnet/runtime/blob/836d1664384b7f7eb3e8b3439038cb06d95846b1/src/libraries/System.Private.CoreLib/src/System/Random.Xoshiro256StarStarImpl.cs#L91C30-L91C30
    public int Next(int maxValue)
    {
        Debug.Assert(maxValue >= 0);

        return (int)NextUInt32((uint)maxValue);
    }

    // https://github.com/dotnet/runtime/blob/836d1664384b7f7eb3e8b3439038cb06d95846b1/src/libraries/System.Private.CoreLib/src/System/Random.Xoshiro256StarStarImpl.cs#L98C28-L98C28
    public int Next(int minValue, int maxValue)
    {
        Debug.Assert(minValue <= maxValue);

        return (int)NextUInt32((uint)(maxValue - minValue)) + minValue;
    }

    // https://github.com/dotnet/runtime/blob/836d1664384b7f7eb3e8b3439038cb06d95846b1/src/libraries/System.Private.CoreLib/src/System/Random.Xoshiro256StarStarImpl.cs#L193C29-L193C29
    public float NextSingle() => (NextUInt64() >> 40) * (1.0f / (1u << 24));

    // https://github.com/dotnet/runtime/blob/836d1664384b7f7eb3e8b3439038cb06d95846b1/src/libraries/System.Private.CoreLib/src/System/Random.ImplBase.cs#L39
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint NextUInt32(uint maxValue)
    {
        ulong randomProduct = (ulong)maxValue * Next();
        uint lowPart = (uint)randomProduct;

        if (lowPart < maxValue)
        {
            uint remainder = (0u - maxValue) % maxValue;

            while (lowPart < remainder)
            {
                randomProduct = (ulong)maxValue * Next();
                lowPart = (uint)randomProduct;
            }
        }

        return (uint)(randomProduct >> 32);
    }
}
