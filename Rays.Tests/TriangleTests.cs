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
        var rightLine = new Line(bottomRight, top);
        var leftLine = new Line(top, bottomLeft);
        var bottomLine = new Line(bottomLeft, bottomRight);
        var rightNormal = Vector2.Normalize(new Vector2(-rightLine.Direction.Y, rightLine.Direction.X));
        var leftNormal = Vector2.Normalize(new Vector2(-leftLine.Direction.Y, leftLine.Direction.X));
        var bottomNormal = Vector2.Normalize(new Vector2(-bottomLine.Direction.Y, bottomLine.Direction.X));
        var expectedWalls = new[]
        {
            new Wall(rightLine, rightNormal),
            new Wall(leftLine, leftNormal),
            new Wall(bottomLine, bottomNormal)
        };
        var triangle = new Triangle(top, bottomLeft, bottomRight);

        var walls = triangle.GetAsWalls();

        Assert.Equal(expectedWalls, walls);
    }
}