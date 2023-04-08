namespace Rays.GeometryLoaders.Materials;

public sealed record Deformity(string? BumpMapTextureFileName, string? DisplacementTextureMapFileName, string? StencilDecalTextureMapFileName)
{
    internal static Deformity Parse(Dictionary<string, string> lines)
    {
        string? bumpMapTextureFileName = null;
        if (lines.TryGetValue("map_bump", out string? line))
        {
            bumpMapTextureFileName = line;
        }
        else if (lines.TryGetValue("bump", out line))
        {
            bumpMapTextureFileName = line;
        }

        string? displacementTextureMapFileName = null;
        if (lines.TryGetValue("disp", out line))
        {
            displacementTextureMapFileName = line;
        }

        string? stencilDecalTextureMapFileName = null;
        if (lines.TryGetValue("decal", out line))
        {
            stencilDecalTextureMapFileName = line;
        }

        return new Deformity(bumpMapTextureFileName, displacementTextureMapFileName, stencilDecalTextureMapFileName);
    }
}
