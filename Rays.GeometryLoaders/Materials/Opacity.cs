using CommunityToolkit.HighPerformance;
using System.Globalization;
using System.Numerics;

namespace Rays.GeometryLoaders.Materials;

public sealed record Opacity(float? Transparency, Vector3? TransmissionFilterColor, float? OpticalDensity, string? TextureMapFileName)
{
    internal static Opacity Parse(Dictionary<string, string> lines, string folderPath)
    {
        float? transparency = null;
        if (lines.TryGetValue("Tr", out string? line))
        {
            transparency = float.Parse(line, CultureInfo.InvariantCulture);
        }

        Vector3? transmissionFilterColor = null;
        if (lines.TryGetValue("Tf", out line))
        {
            transmissionFilterColor = line.Tokenize(' ').ParseVector3();
        }

        float? opticalDensity = null;
        if (lines.TryGetValue("Ni", out line))
        {
            opticalDensity = float.Parse(line, CultureInfo.InvariantCulture);
        }

        string? textureMapFileName = null;
        if (lines.TryGetValue("map_d", out line))
        {
            textureMapFileName = Path.Combine(folderPath, line);
        }

        return new Opacity(transparency, transmissionFilterColor, opticalDensity, textureMapFileName);
    }
}
