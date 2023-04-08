using System.Numerics;
using Rays.GeometryLoaders.Geometry;

namespace Rays.GeometryLoaders.Tests.Geometry;

public sealed class GeometryObjectBuilderTests
{
    [Fact]
    public void CreateFromString_WithVertex_CreatesExpectedVertices()
    {
        var expectedVertices = new[]
        {
            new Vertex(new Vector4(0.0f, 0.0f, 0.0f, 1.0f)),
        };
        string objContent = @"
v 0.0 0.0 0.0";

        GeometryObject actual = GeometryObjectBuilder.CreateFromString(objContent);

        Assert.Equal(expectedVertices, actual.Vertices);
    }

    [Fact]
    public void CreateFromString_WithNormal_CreatesExpectedNormals()
    {
        var expectedNormals = new[]
        {
            new VertexNormal(new Vector3(0.0f, 0.0f, 1.0f))
        };
        string objContent = @"
vn 0.0 0.0 1.0";

        GeometryObject actual = GeometryObjectBuilder.CreateFromString(objContent);

        Assert.Equal(expectedNormals, actual.Normals);
    }

    [Fact]
    public void CreateFromString_WithTextureCoordinate_CreatesExpectedTextureCoordinates()
    {
        var expectedTextureCoordinates = new[]
        {
            new TextureCoordinate(new Vector3(0.0f, 0.0f, 0.0f))
        };
        string objContent = @"
vt 0.0 0.0";

        GeometryObject actual = GeometryObjectBuilder.CreateFromString(objContent);

        Assert.Equal(expectedTextureCoordinates, actual.TextureCoordinates);
    }

    [Fact]
    public void CreateFromString_WithFace_CreatesExpectedFaces()
    {
        var expectedFaces = new Face[]
        {
            new Face(new[] { 1 }, new[] { 2 }, new[] { 3 })
        };
        string input = @"
g testModel
f 1/2/3";

        GeometryObject actual = GeometryObjectBuilder.CreateFromString(input);

        Assert.Equal(expectedFaces.Length, actual.GeometryModels[0].Faces.Length);
        Assert.Equal(expectedFaces[0].VertexIndexes, actual.GeometryModels[0].Faces[0].VertexIndexes);
        Assert.Equal(expectedFaces[0].TextureCoordinateIndexes, actual.GeometryModels[0].Faces[0].TextureCoordinateIndexes);
        Assert.Equal(expectedFaces[0].NormalIndexes, actual.GeometryModels[0].Faces[0].NormalIndexes);
    }
}