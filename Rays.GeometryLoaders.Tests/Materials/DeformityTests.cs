using Rays.GeometryLoaders.Materials;

namespace Rays.GeometryLoaders.Tests.Materials;

public sealed class DeformityTests
{
    [Fact]
    public void Parse_WithBumpMapTextureFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "map_bump", "bump_map.png" }
        };
        var expectedDeformity = new Deformity("bump_map.png", null, null);

        var actualDeformity = Deformity.Parse(lines);

        Assert.Equal(expectedDeformity, actualDeformity);
    }

    [Fact]
    public void Parse_WithBumpAlternative_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "bump", "bump_map.png" }
        };
        var expectedDeformity = new Deformity("bump_map.png", null, null);

        var actualDeformity = Deformity.Parse(lines);

        Assert.Equal(expectedDeformity, actualDeformity);
    }

    [Fact]
    public void Parse_WithDisplacementTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "disp", "displacement_map.png" }
        };
        var expectedDeformity = new Deformity(null, "displacement_map.png", null);

        var actualDeformity = Deformity.Parse(lines);

        Assert.Equal(expectedDeformity, actualDeformity);
    }

    [Fact]
    public void Parse_WithStencilDecalTextureMapFileName_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "decal", "stencil_decal_map.png" }
        };
        var expectedDeformity = new Deformity(null, null, "stencil_decal_map.png");

        var actualDeformity = Deformity.Parse(lines);

        Assert.Equal(expectedDeformity, actualDeformity);
    }

    [Fact]
    public void Parse_WithAllProperties_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>
        {
            { "map_bump", "bump_map.png" },
            { "disp", "displacement_map.png" },
            { "decal", "stencil_decal_map.png" }
        };
        var expectedDeformity = new Deformity("bump_map.png", "displacement_map.png", "stencil_decal_map.png");

        var actualDeformity = Deformity.Parse(lines);

        Assert.Equal(expectedDeformity, actualDeformity);
    }

    [Fact]
    public void Parse_WithoutProperties_ParsesCorrectly()
    {
        var lines = new Dictionary<string, string>();
        var expectedDeformity = new Deformity(null, null, null);

        var actualDeformity = Deformity.Parse(lines);

        Assert.Equal(expectedDeformity, actualDeformity);
    }
}