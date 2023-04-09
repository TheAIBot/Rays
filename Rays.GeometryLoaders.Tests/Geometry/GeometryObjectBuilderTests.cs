using Rays.GeometryLoaders.Geometry;
using Rays.GeometryLoaders.Materials;
using System.Numerics;

namespace Rays.GeometryLoaders.Tests.Geometry;

public sealed class GeometryObjectBuilderTests
{
    [Fact]
    public void CreateFromFile_WithVertex_CreatesExpectedVertices()
    {
        var expectedVertices = new[]
        {
            new Vertex(new Vector4(0.0f, 0.0f, 0.0f, 1.0f)),
        };
        var objContent = "v 0.0 0.0 0.0";
        using var objFile = new TemporaryFile(objContent);

        var geometryObject = GeometryObjectBuilder.CreateFromFile(objFile.FileName);

        Assert.Equal(expectedVertices, geometryObject.Vertices);
    }

    [Fact]
    public void CreateFromFile_WithNormal_CreatesExpectedNormals()
    {
        var expectedNormals = new[]
        {
            new VertexNormal(new Vector3(0.0f, 0.0f, 1.0f))
        };
        var objContent = "vn 0.0 0.0 1.0";
        using var objFile = new TemporaryFile(objContent);

        var geometryObject = GeometryObjectBuilder.CreateFromFile(objFile.FileName);

        Assert.Equal(expectedNormals, geometryObject.Normals);
    }

    [Fact]
    public void CreateFromFile_WithTextureCoordinate_CreatesExpectedTextureCoordinates()
    {
        var expectedTextureCoordinates = new[]
        {
            new TextureCoordinate(new Vector3(0.0f, 0.0f, 0.0f))
        };
        var objContent = "vt 0.0 0.0";
        using var objFile = new TemporaryFile(objContent);

        var geometryObject = GeometryObjectBuilder.CreateFromFile(objFile.FileName);

        Assert.Equal(expectedTextureCoordinates, geometryObject.TextureCoordinates);
    }

    [Fact]
    public void CreateFromFile_WithFace_CreatesExpectedFaces()
    {
        var expectedFaces = new Face[]
        {
            new Face(new[] { 1 }, new[] { 2 }, new[] { 3 })
        };
        var mtlContent = @"
newmtl material
Ka 0.2 0.3 0.4";
        using var mtlFile = new TemporaryFile(mtlContent);
        var objContent = $@"
g testModel
mtllib {Path.GetFileName(mtlFile.FileName)}
usemtl material
f 1/2/3";
        using var objFile = new TemporaryFile(objContent);

        var geometryObject = GeometryObjectBuilder.CreateFromFile(objFile.FileName);

        Assert.Equal(expectedFaces.Length, geometryObject.GeometryModels[0].Faces.Length);
        Assert.Equal(expectedFaces[0].VertexIndexes, geometryObject.GeometryModels[0].Faces[0].VertexIndexes);
        Assert.Equal(expectedFaces[0].TextureCoordinateIndexes, geometryObject.GeometryModels[0].Faces[0].TextureCoordinateIndexes);
        Assert.Equal(expectedFaces[0].NormalIndexes, geometryObject.GeometryModels[0].Faces[0].NormalIndexes);
    }

    [Fact]
    public void CreateFromFile_WithOneMaterial_SetsCorrectMaterial()
    {
        var expectedMaterial = new Material("material",
                                            new Ambient(new Vector3(0.2f, 0.3f, 0.4f), null),
                                            new(null, null),
                                            new(null, null, null, null),
                                            new(null, null, null, null),
                                            new(null, null, null));
        var mtlContent = @"
newmtl material
Ka 0.2 0.3 0.4";
        using var mtlFile = new TemporaryFile(mtlContent);
        var objContent = $@"
g testModel
mtllib {Path.GetFileName(mtlFile.FileName)}
usemtl material
f 1/2/3";
        using var objFile = new TemporaryFile(objContent);

        var geometryObject = GeometryObjectBuilder.CreateFromFile(objFile.FileName);
        var actualMaterial = geometryObject.GeometryModels[0].Material;

        Assert.Equal(expectedMaterial, actualMaterial);
    }

    [Fact]
    public void CreateFromFile_WithTwoMaterialsInDifferentGeometryModels_SetsCorrectMaterials()
    {
        var expectedMaterial1 = new Material("material1", new Ambient(new Vector3(0.2f, 0.3f, 0.4f), null),
                                             new(null, null),
                                             new(null, null, null, null),
                                             new(null, null, null, null),
                                             new(null, null, null));
        var expectedMaterial2 = new Material("material2", new Ambient(new Vector3(0.5f, 0.6f, 0.7f), null),
                                             new(null, null),
                                             new(null, null, null, null),
                                             new(null, null, null, null),
                                             new(null, null, null));
        var mtlContent = @"
newmtl material1
Ka 0.2 0.3 0.4
newmtl material2
Ka 0.5 0.6 0.7";
        using var mtlFile = new TemporaryFile(mtlContent);
        var objContent = $@"
g testModel1
mtllib {Path.GetFileName(mtlFile.FileName)}
usemtl material1
f 1/2/3
g testModel2
usemtl material2
f 4/5/6";
        using var objFile = new TemporaryFile(objContent);

        var geometryObject = GeometryObjectBuilder.CreateFromFile(objFile.FileName);
        var actualMaterial1 = geometryObject.GeometryModels[0].Material;
        var actualMaterial2 = geometryObject.GeometryModels[1].Material;

        Assert.Equal(expectedMaterial1, actualMaterial1);
        Assert.Equal(expectedMaterial2, actualMaterial2);
    }

}
