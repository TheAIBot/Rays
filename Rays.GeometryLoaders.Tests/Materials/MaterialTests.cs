using Rays.GeometryLoaders.Materials;
using System.Numerics;

namespace Rays.GeometryLoaders.Tests.Materials;

public sealed class MaterialTests
{
    [Fact]
    public void CreateFromString_SingleMaterial_ParsesCorrectly()
    {
        string content = @"
newmtl TestMaterial
Ka 0.2 0.3 0.4
Kd 0.5 0.6 0.7
Ks 0.8 0.9 1.0
Ns 200
Tr 0.1
Ni 1.5
Tf 0.5 0.5 0.5
map_Ka ambient_map.png
map_kd diffusion_map.png
map_Ks specular_map.png
map_Ns highlight_map.png
map_d opacity_map.png
map_bump bump_map.png
disp displacement_map.png
decal decal_map.png";
        var expectedMaterial = new Material(
            "TestMaterial",
            new Ambient(new Vector3(0.2f, 0.3f, 0.4f), "ambient_map.png"),
            new Diffusion(new Vector3(0.5f, 0.6f, 0.7f), "diffusion_map.png"),
            new Specular(new Vector3(0.8f, 0.9f, 1.0f), 200, "specular_map.png", "highlight_map.png"),
            new Opacity(0.1f, new Vector3(0.5f, 0.5f, 0.5f), 1.5f, "opacity_map.png"),
            new Deformity("bump_map.png", "displacement_map.png", "decal_map.png"));

        var actualMaterials = Material.CreateFromString(content);

        Assert.Collection(actualMaterials, material => Assert.Equal(expectedMaterial, material));
    }

    [Fact]
    public void CreateFromString_MultipleMaterials_ParsesCorrectly()
    {
        string content = @"
newmtl Material1
Ka 0.1 0.2 0.3
map_Ka ambient1.png

newmtl Material2
Kd 0.4 0.5 0.6
map_kd diffusion2.png";
        var expectedMaterials = new List<Material>
        {
            new Material(
                "Material1",
                new Ambient(new Vector3(0.1f, 0.2f, 0.3f), "ambient1.png"),
                new Diffusion(null, null),
                new Specular(null, null, null, null),
                new Opacity(null, null, null, null),
                new Deformity(null, null, null)),
            new Material(
                "Material2",
                new Ambient(null, null),
                new Diffusion(new Vector3(0.4f, 0.5f, 0.6f), "diffusion2.png"),
                new Specular(null, null, null, null),
                new Opacity(null, null, null, null),
                new Deformity(null, null, null))
        };

        var actualMaterials = Material.CreateFromString(content);

        Assert.Equal(expectedMaterials, actualMaterials);
    }

    [Fact]
    public void CreateFromString_WithEmptyLinesAndComments_ParsesCorrectly()
    {
        string content = @"# Comment
newmtl TestMaterial

# Comment
Ka 0.2 0.3 0.4

# Comment
Kd 0.5 0.6 0.7";
        var expectedMaterial = new Material(
            "TestMaterial",
            new Ambient(new Vector3(0.2f, 0.3f, 0.4f), null),
            new Diffusion(new Vector3(0.5f, 0.6f, 0.7f), null),
            new Specular(null, null, null, null),
            new Opacity(null, null, null, null),
            new Deformity(null, null, null));

        var actualMaterials = Material.CreateFromString(content);

        Assert.Collection(actualMaterials, material => Assert.Equal(expectedMaterial, material));
    }
}