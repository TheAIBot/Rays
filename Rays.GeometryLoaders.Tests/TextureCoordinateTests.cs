using System.Numerics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;

namespace Rays.GeometryLoaders.Tests;

public sealed class TextureCoordinateTests
{
    [Fact]
    public void Parse_WhenInputHasOnlyU_CreatesTextureCoordinateWithUAndDefaultVW()
    {
        var inputLine = "vt 0.5";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = TextureCoordinate.Parse(lineTokens);

        Assert.Equal(new Vector3(0.5f, 0.0f, 0.0f), result.Coordinate);
    }

    [Fact]
    public void Parse_WhenInputHasUV_CreatesTextureCoordinateWithUVAndDefaultW()
    {
        var inputLine = "vt 0.5 0.3";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = TextureCoordinate.Parse(lineTokens);

        Assert.Equal(new Vector3(0.5f, 0.3f, 0.0f), result.Coordinate);
    }

    [Fact]
    public void Parse_WhenInputHasUVW_CreatesTextureCoordinateWithUVW()
    {
        var inputLine = "vt 0.5 0.3 0.8";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        var result = TextureCoordinate.Parse(lineTokens);

        Assert.Equal(new Vector3(0.5f, 0.3f, 0.8f), result.Coordinate);
    }

    [Fact]
    public void Parse_WhenInputHasInvalidTokens_ThrowsFormatException()
    {
        var inputLine = "vt 0.5 invalid";
        var lineTokens = new ReadOnlySpanTokenizer<char>(inputLine.AsSpan(), ' ');
        lineTokens.MoveNext();
        lineTokens.MoveNext();

        Exception? caughtException = null;

        try
        {
            TextureCoordinate.Parse(lineTokens);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        Assert.IsType<FormatException>(caughtException);
    }
}

