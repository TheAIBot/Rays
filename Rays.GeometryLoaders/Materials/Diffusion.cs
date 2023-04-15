using CommunityToolkit.HighPerformance;
using System.Numerics;

namespace Rays.GeometryLoaders.Materials;

public sealed record Diffusion(Vector3? Color, string? TextureMapFileName)
{
    internal static Diffusion Parse(Dictionary<string, string> lines, string folderPath)
    {
        Vector3? color = null;
        if (lines.TryGetValue("Kd", out string? line))
        {
            color = line.Tokenize(' ').ParseVector3();
        }

        string? textureMapFileName = null;
        if (lines.TryGetValue("map_Kd", out line))
        {
            textureMapFileName = Path.Combine(folderPath, line);
        }

        return new Diffusion(color, textureMapFileName);
    }
}
