using CommunityToolkit.HighPerformance.Enumerables;
using System.Globalization;
using System.Numerics;

namespace Rays.GeometryLoaders;

internal static class VectorExtensions
{
    public static Vector3 ParseVector3(this ReadOnlySpanTokenizer<char> tokens)
    {
        if (!tokens.MoveNext())
        {
            throw new InvalidOperationException();
        }
        float x = float.Parse(tokens.Current, CultureInfo.InvariantCulture);

        if (!tokens.MoveNext())
        {
            throw new InvalidOperationException();
        }
        float y = float.Parse(tokens.Current, CultureInfo.InvariantCulture);

        if (!tokens.MoveNext())
        {
            throw new InvalidOperationException();
        }
        float z = float.Parse(tokens.Current, CultureInfo.InvariantCulture);

        return new Vector3(x, y, z);
    }
}
