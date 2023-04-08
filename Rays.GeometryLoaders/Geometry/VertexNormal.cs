using System.Globalization;
using System.Numerics;
using CommunityToolkit.HighPerformance.Enumerables;

namespace Rays.GeometryLoaders.Geometry;

public readonly record struct VertexNormal(Vector3 Normal)
{
    internal const string LinePrefix = "vn";

    internal static VertexNormal Parse(ReadOnlySpanTokenizer<char> lineTokens)
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

        return new VertexNormal(new Vector3(x, y, z));
    }
}
