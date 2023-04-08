using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;
using Rays.GeometryLoaders.Geometry;
using System.Numerics;

namespace Rays.GeometryLoaders.Tests.Geometry;

public sealed class VertexTests
{
    [Fact]
    public void Parse_WhenInputHasXYZ_CreatesVertexWithXYZAndDefaultW()
    {
        var inputLine = "v 1.0 2.0 3.0";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = Vertex.Parse(lineTokens);

        Assert.Equal(new Vector4(1.0f, 2.0f, 3.0f, 1.0f), result.Value);
    }

    [Fact]
    public void Parse_WhenInputHasXYZW_CreatesVertexWithXYZW()
    {
        var inputLine = "v 1.0 2.0 3.0 4.0";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = Vertex.Parse(lineTokens);

        Assert.Equal(new Vector4(1.0f, 2.0f, 3.0f, 4.0f), result.Value);
    }

    [Fact]
    public void Parse_WhenInputHasNotEnoughTokens_ThrowsInvalidOperationException()
    {
        var inputLine = "v 1.0 2.0";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();
        Exception? caughtException = null;

        try
        {
            Vertex.Parse(lineTokens);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.IsType<InvalidOperationException>(caughtException);
    }

    [Fact]
    public void Parse_WhenInputHasInvalidTokens_ThrowsFormatException()
    {
        var inputLine = "v 1.0 2.0 invalid";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();
        Exception? caughtException = null;

        try
        {
            Vertex.Parse(lineTokens);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.IsType<FormatException>(caughtException);
    }
}