using Rays.Polygons;
using System.Numerics;

namespace Rays.Tests;

public sealed class RectangleTests
{
    [Fact]
    public void GetAsWalls_WallsCreatedFromRectangleVertices()
    {
        var bottomLeft = new Vector2(0, 0);
        var horizontal = new Vector2(2, 0);
        var vertical = new Vector2(0, 1);
        var rectangle = new Rectangle(bottomLeft, horizontal, vertical);
        var expectedWalls = new[]
        {
            new Wall(new Line(rectangle.TopLeft, rectangle.TopRight), new Vector2(0, 1)),
            new Wall(new Line(rectangle.BottomRight, rectangle.BottomLeft), new Vector2(0, -1)),
            new Wall(new Line(rectangle.BottomLeft, rectangle.TopLeft), new Vector2(-1, 0)),
            new Wall(new Line(rectangle.TopRight, rectangle.BottomRight), new Vector2(1, 0))
        };

        var walls = rectangle.GetAsWalls();

        Assert.Equal(expectedWalls, walls);
    }

    [Fact]
    public void RotateRectangle_RotatesRectangleByGivenAngle()
    {
        var bottomLeft = new Vector2(0, 0);
        var horizontal = new Vector2(2, 0);
        var vertical = new Vector2(0, 1);
        var rectangle = new Rectangle(bottomLeft, horizontal, vertical);
        float angle = 90f;

        var rotatedRectangle = Rectangle.RotateRectangle(rectangle, angle);

        float tolerance = 0.001f;
        Assert.Equal(rectangle.Horizontal.Length(), rotatedRectangle.Horizontal.Length(), 0.00001f);
        Assert.Equal(rectangle.Vertical.Length(), rotatedRectangle.Vertical.Length(), 0.00001f);
        Assert.Equal(rectangle.Center.X, rotatedRectangle.Center.X, tolerance);
        Assert.Equal(rectangle.Center.Y, rotatedRectangle.Center.Y, tolerance);
    }
}
