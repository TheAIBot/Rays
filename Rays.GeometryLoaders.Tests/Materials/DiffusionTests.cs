using Rays.GeometryLoaders.Materials;
using System.Numerics;

namespace Rays.GeometryLoaders.Tests.Materials;

public sealed class DiffusionTests
{
    [Fact]
    public void Parse_WithColor_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Kd", "0.2 0.4 0.6" }
        };
        var expectedDiffusion = new Diffusion(new Vector3(0.2f, 0.4f, 0.6f), null);

        var actualDiffusion = Diffusion.Parse(lines);

        Assert.Equal(expectedDiffusion, actualDiffusion);
    }

    [Fact]
    public void Parse_WithTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "map_kd", "texture_map.png" }
        };
        var expectedDiffusion = new Diffusion(null, "texture_map.png");

        var actualDiffusion = Diffusion.Parse(lines);

        Assert.Equal(expectedDiffusion, actualDiffusion);
    }

    [Fact]
    public void Parse_WithColorAndTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Kd", "0.2 0.4 0.6" },
            { "map_kd", "texture_map.png" }
        };
        var expectedDiffusion = new Diffusion(new Vector3(0.2f, 0.4f, 0.6f), "texture_map.png");

        var actualDiffusion = Diffusion.Parse(lines);

        Assert.Equal(expectedDiffusion, actualDiffusion);
    }

    [Fact]
    public void Parse_WithoutColorAndTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>();
        var expectedDiffusion = new Diffusion(null, null);

        var actualDiffusion = Diffusion.Parse(lines);

        Assert.Equal(expectedDiffusion, actualDiffusion);
    }
}