using Rays.GeometryLoaders.Materials;
using System.Numerics;

namespace Rays.GeometryLoaders.Tests.Materials;

public sealed class AmbientTests
{
    [Fact]
    public void Parse_WithColor_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Ka", "0.2 0.4 0.6" }
        };
        var expectedAmbient = new Ambient(new Vector3(0.2f, 0.4f, 0.6f), null);

        var actualAmbient = Ambient.Parse(lines, "e");

        Assert.Equal(expectedAmbient, actualAmbient);
    }

    [Fact]
    public void Parse_WithTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "map_Ka", "texture_map.png" }
        };
        var expectedAmbient = new Ambient(null, Path.Combine("e", "texture_map.png"));

        var actualAmbient = Ambient.Parse(lines, "e");

        Assert.Equal(expectedAmbient, actualAmbient);
    }

    [Fact]
    public void Parse_WithColorAndTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Ka", "0.2 0.4 0.6" },
            { "map_Ka", "texture_map.png" }
        };
        var expectedAmbient = new Ambient(new Vector3(0.2f, 0.4f, 0.6f), Path.Combine("e", "texture_map.png"));

        var actualAmbient = Ambient.Parse(lines, "e");

        Assert.Equal(expectedAmbient, actualAmbient);
    }

    [Fact]
    public void Parse_WithoutColorAndTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>();
        var expectedAmbient = new Ambient(null, null);

        var actualAmbient = Ambient.Parse(lines, "e");

        Assert.Equal(expectedAmbient, actualAmbient);
    }
}