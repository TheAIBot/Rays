using CommunityToolkit.HighPerformance;
using System.Globalization;
using System.Numerics;

namespace Rays.GeometryLoaders.Materials;

public sealed record Specular(Vector3? Color, float? Exponent, string? TextureMapFileName, string? HighlightTextureMapFileName)
{
    internal static Specular Parse(Dictionary<string, string> lines)
    {
        Vector3? color = null;
        if (lines.TryGetValue("Ks", out string? line))
        {
            color = line.Tokenize(' ').ParseVector3();
        }

        float? exponent = null;
        if (lines.TryGetValue("Ns", out line))
        {
            exponent = float.Parse(line, CultureInfo.InvariantCulture);
        }

        string? textureMapFileName = null;
        if (lines.TryGetValue("map_Ks", out line))
        {
            textureMapFileName = line;
        }

        string? highlightTextureMapFileName = null;
        if (lines.TryGetValue("map_Ns", out line))
        {
            highlightTextureMapFileName = line;
        }

        return new Specular(color, exponent, textureMapFileName, highlightTextureMapFileName);
    }
}
