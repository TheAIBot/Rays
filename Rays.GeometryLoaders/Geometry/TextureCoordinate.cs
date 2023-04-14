using CommunityToolkit.HighPerformance.Enumerables;
using System.Globalization;
using System.Numerics;

namespace Rays.GeometryLoaders.Geometry;

public readonly record struct TextureCoordinate(Vector3 Coordinate)
{
    internal const string LinePrefix = "vt";

    internal static TextureCoordinate Parse(ReadOnlySpanTokenizer<char> lineTokens)
    {
        float u = float.Parse(lineTokens.Current, CultureInfo.InvariantCulture);

        float v = 0.0f;
        if (lineTokens.MoveNextNonEmpty())
        {
            v = float.Parse(lineTokens.Current, CultureInfo.InvariantCulture);
        }

        float w = 0.0f;
        if (lineTokens.MoveNextNonEmpty())
        {
            w = float.Parse(lineTokens.Current, CultureInfo.InvariantCulture);
        }

        return new TextureCoordinate(new Vector3(u, v, w));
    }
}
