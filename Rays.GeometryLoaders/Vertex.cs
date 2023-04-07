using System.Globalization;
using System.Numerics;
using CommunityToolkit.HighPerformance.Enumerables;

namespace Rays.GeometryLoaders;

public readonly record struct Vertex(Vector4 Value)
{
    internal const string LinePrefix = "v";

    internal static Vertex Parse(ReadOnlySpanTokenizer<char> lineTokens)
    {
        float x = float.Parse(lineTokens.Current, CultureInfo.InvariantCulture);

        if (!lineTokens.MoveNext())
        {
            throw new InvalidOperationException();
        }
        float y = float.Parse(lineTokens.Current, CultureInfo.InvariantCulture);

        if (!lineTokens.MoveNext())
        {
            throw new InvalidOperationException();
        }
        float z = float.Parse(lineTokens.Current, CultureInfo.InvariantCulture);

        // w is optional with default value of 1.0
        float w = 1.0f;
        if (lineTokens.MoveNext())
        {
            w = float.Parse(lineTokens.Current, CultureInfo.InvariantCulture);
        }

        return new Vertex(new Vector4(x, y, z, w));
    }
}
