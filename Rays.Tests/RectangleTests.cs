using Rays.Polygons;
using System.Numerics;

namespace Rays.Tests;

public class RectangleTests
{
    [Fact]
    public void TestCorners()
    {
        var bottomLeft = new Vector2(0, 0);
        var horizontal = new Vector2(10, 0);
        var vertical = new Vector2(0, 5);
        var rect = new Rectangle(bottomLeft, horizontal, vertical);

        Assert.Equal(bottomLeft, rect.BottomLeft);
        Assert.Equal(new Vector2(0, 5), rect.TopLeft);
        Assert.Equal(new Vector2(10, 0), rect.BottomRight);
        Assert.Equal(new Vector2(10, 5), rect.TopRight);
    }

    [Fact]
    public void TestCenter()
    {
        var bottomLeft = new Vector2(0, 0);
        var horizontal = new Vector2(10, 0);
        var vertical = new Vector2(0, 5);
        var rect = new Rectangle(bottomLeft, horizontal, vertical);

        Assert.Equal(new Vector2(5, 2.5f), rect.Center);
    }

    [Fact]
    public void TestGetAsWalls()
    {
        var bottomLeft = new Vector2(0, 0);
        var horizontal = new Vector2(10, 0);
        var vertical = new Vector2(0, 5);
        var rect = new Rectangle(bottomLeft, horizontal, vertical);

        Wall[] walls = rect.GetAsWalls();

        Assert.Equal(4, walls.Length);
        Assert.Equal(new Line(new Vector2(0, 5), new Vector2(10, 5)), walls[0].Line);
        Assert.Equal(new Line(new Vector2(10, 0), new Vector2(0, 0)), walls[1].Line);
        Assert.Equal(new Line(new Vector2(0, 0), new Vector2(0, 5)), walls[2].Line);
        Assert.Equal(new Line(new Vector2(10, 5), new Vector2(10, 0)), walls[3].Line);
    }
}