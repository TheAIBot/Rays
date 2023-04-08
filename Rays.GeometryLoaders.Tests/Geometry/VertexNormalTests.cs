using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;
using Rays.GeometryLoaders.Geometry;

namespace Rays.GeometryLoaders.Tests.Geometry;

public sealed class VertexNormalTests
{
    [Fact]
    public void Parse_WhenInputHasXYZ_CreatesVertexNormalWithXYZ()
    {
        var inputLine = "vn 1.0 2.0 3.0";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = VertexNormal.Parse(lineTokens);

        Assert.Equal(new Vector3(1.0f, 2.0f, 3.0f), result.Normal);
    }

    [Fact]
    public void Parse_WhenInputHasNotEnoughTokens_ThrowsInvalidOperationException()
    {
        var inputLine = "vn 1.0 2.0";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        Exception? caughtException = null;

        try
        {
            VertexNormal.Parse(lineTokens);
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
        var inputLine = "vn 1.0 2.0 invalid";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        Exception? caughtException = null;

        try
        {
            VertexNormal.Parse(lineTokens);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.IsType<FormatException>(caughtException);
    }
}
