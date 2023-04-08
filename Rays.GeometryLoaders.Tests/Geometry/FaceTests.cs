using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;
using Rays.GeometryLoaders.Geometry;

namespace Rays.GeometryLoaders.Tests.Geometry;

public sealed class FaceTests
{
    [Fact]
    public void Parse_WhenInputHasVertexIndexes_CreatesFaceWithVertexIndexes()
    {
        var inputLine = "f 1 2 3";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = Face.Parse(lineTokens);

        Assert.Equal(new int[] { 1, 2, 3 }, result.VertexIndexes);
        Assert.Null(result.TextureCoordinateIndexes);
        Assert.Null(result.NormalIndexes);
    }

    [Fact]
    public void Parse_WhenInputHasVertexAndTextureCoordinateIndexes_CreatesFaceWithVertexAndTextureCoordinateIndexes()
    {
        var inputLine = "f 1/4 2/5 3/6";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = Face.Parse(lineTokens);

        Assert.Equal(new int[] { 1, 2, 3 }, result.VertexIndexes);
        Assert.Equal(new int[] { 4, 5, 6 }, result.TextureCoordinateIndexes);
        Assert.Null(result.NormalIndexes);
    }

    [Fact]
    public void Parse_WhenInputHasVertexAndNormalIndexes_CreatesFaceWithVertexAndNormalIndexes()
    {
        var inputLine = "f 1//7 2//8 3//9";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = Face.Parse(lineTokens);

        Assert.Equal(new int[] { 1, 2, 3 }, result.VertexIndexes);
        Assert.Null(result.TextureCoordinateIndexes);
        Assert.Equal(new int[] { 7, 8, 9 }, result.NormalIndexes);
    }

    [Fact]
    public void Parse_WhenInputHasVertexTextureCoordinateAndNormalIndexes_CreatesFaceWithVertexTextureCoordinateAndNormalIndexes()
    {
        var inputLine = "f 1/4/7 2/5/8 3/6/9";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = Face.Parse(lineTokens);

        Assert.Equal(new int[] { 1, 2, 3 }, result.VertexIndexes);
        Assert.Equal(new int[] { 4, 5, 6 }, result.TextureCoordinateIndexes);
        Assert.Equal(new int[] { 7, 8, 9 }, result.NormalIndexes);
    }

    [Fact]
    public void Parse_WhenInputHasInvalidTokens_ThrowsFormatException()
    {
        var inputLine = "f 1/4/invalid 2/5/8 3/6/9";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        Exception? caughtException = null;

        try
        {
            Face.Parse(lineTokens);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.IsType<FormatException>(caughtException);
    }
}
