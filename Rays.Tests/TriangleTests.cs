using Rays.Polygons;
using System.Numerics;

namespace Rays.Tests;

public sealed class TriangleTests
{
    [Fact]
    public void GetAsWalls_WallsCreatedFromTriangleVertices()
    {
        var top = new Vector2(0, 1);
        var bottomLeft = new Vector2(-1, 0);
        var bottomRight = new Vector2(1, 0);
        var expectedWalls = new[]
        {
            new Wall(new Line(bottomRight, top), Vector2.Zero),
            new Wall(new Line(top, bottomLeft), Vector2.Zero),
            new Wall(new Line(bottomLeft, bottomRight), Vector2.Zero)
        };
        var triangle = new Triangle(top, bottomLeft, bottomRight);

        var walls = triangle.GetAsWalls();

        Assert.Equal(expectedWalls, walls);
    }
}