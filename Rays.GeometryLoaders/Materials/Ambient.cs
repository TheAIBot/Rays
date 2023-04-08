using CommunityToolkit.HighPerformance;
using System.Numerics;

namespace Rays.GeometryLoaders.Materials;

public sealed record Ambient(Vector3? Color, string? TextureMapFileName)
{
    internal static Ambient Parse(Dictionary<string, string> lines)
    {
        Vector3? color = null;
        if (lines.TryGetValue("Ka", out string? line))
        {
            color = line.Tokenize(' ').ParseVector3();
        }

        string? textureMapFileName = null;
        if (lines.TryGetValue("map_Ka", out line))
        {
            textureMapFileName = line;
        }

        return new Ambient(color, textureMapFileName);
    }
}
