using Rays.GeometryLoaders.Materials;
using System.Numerics;

namespace Rays.GeometryLoaders.Tests.Materials;

public sealed class SpecularTests
{
    [Fact]
    public void Parse_WithColor_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Ks", "0.2 0.4 0.6" }
        };
        var expectedSpecular = new Specular(new Vector3(0.2f, 0.4f, 0.6f), null, null, null);

        var actualSpecular = Specular.Parse(lines);

        Assert.Equal(expectedSpecular, actualSpecular);
    }

    [Fact]
    public void Parse_WithExponent_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Ns", "50" }
        };
        var expectedSpecular = new Specular(null, 50, null, null);

        var actualSpecular = Specular.Parse(lines);

        Assert.Equal(expectedSpecular, actualSpecular);
    }

    [Fact]
    public void Parse_WithTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "map_Ks", "texture_map.png" }
        };
        var expectedSpecular = new Specular(null, null, "texture_map.png", null);

        var actualSpecular = Specular.Parse(lines);

        Assert.Equal(expectedSpecular, actualSpecular);
    }

    [Fact]
    public void Parse_WithHighlightTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "map_Ns", "highlight_texture_map.png" }
        };
        var expectedSpecular = new Specular(null, null, null, "highlight_texture_map.png");

        var actualSpecular = Specular.Parse(lines);

        Assert.Equal(expectedSpecular, actualSpecular);
    }

    [Fact]
    public void Parse_WithAllProperties_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "Ks", "0.2 0.4 0.6" },
            { "Ns", "50" },
            { "map_Ks", "texture_map.png" },
            { "map_Ns", "highlight_texture_map.png" }
        };
        var expectedSpecular = new Specular(new Vector3(0.2f, 0.4f, 0.6f), 50, "texture_map.png", "highlight_texture_map.png");

        var actualSpecular = Specular.Parse(lines);

        Assert.Equal(expectedSpecular, actualSpecular);
    }

    [Fact]
    public void Parse_WithoutProperties_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>();
        var expectedSpecular = new Specular(null, null, null, null);

        var actualSpecular = Specular.Parse(lines);

        Assert.Equal(expectedSpecular, actualSpecular);
    }
}