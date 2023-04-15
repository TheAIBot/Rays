using Rays.GeometryLoaders.Materials;
using System.Numerics;

namespace Rays.GeometryLoaders.Tests.Materials;

public sealed class OpacityTests
{
    [Fact]
    public void Parse_WithTransparency_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Tr", "0.8" }
        };
        var expectedOpacity = new Opacity(0.8f, null, null, null);

        var actualOpacity = Opacity.Parse(lines, "e");

        Assert.Equal(expectedOpacity, actualOpacity);
    }

    [Fact]
    public void Parse_WithTransmissionFilterColor_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Tf", "0.2 0.4 0.6" }
        };
        var expectedOpacity = new Opacity(null, new Vector3(0.2f, 0.4f, 0.6f), null, null);

        var actualOpacity = Opacity.Parse(lines, "e");

        Assert.Equal(expectedOpacity, actualOpacity);
    }

    [Fact]
    public void Parse_WithOpticalDensity_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Ni", "1.5" }
        };
        var expectedOpacity = new Opacity(null, null, 1.5f, null);

        var actualOpacity = Opacity.Parse(lines, "e");

        Assert.Equal(expectedOpacity, actualOpacity);
    }

    [Fact]
    public void Parse_WithTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "map_d", "texture_map.png" }
        };
        var expectedOpacity = new Opacity(null, null, null, Path.Combine("e", "texture_map.png"));

        var actualOpacity = Opacity.Parse(lines, "e");

        Assert.Equal(expectedOpacity, actualOpacity);
    }

    [Fact]
    public void Parse_WithAllProperties_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Tr", "0.8" },
            { "Tf", "0.2 0.4 0.6" },
            { "Ni", "1.5" },
            { "map_d", "texture_map.png" }
        };
        var expectedOpacity = new Opacity(0.8f, new Vector3(0.2f, 0.4f, 0.6f), 1.5f, Path.Combine("e", "texture_map.png"));

        var actualOpacity = Opacity.Parse(lines, "e");

        Assert.Equal(expectedOpacity, actualOpacity);
    }

    [Fact]
    public void Parse_WithoutProperties_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>();
        var expectedOpacity = new Opacity(null, null, null, null);

        var actualOpacity = Opacity.Parse(lines, "e");

        Assert.Equal(expectedOpacity, actualOpacity);
    }
}