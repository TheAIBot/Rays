using System.Numerics;

namespace Rays.Polygons;

public readonly struct Triangle
{
    public readonly Vector2 Top;
    public readonly Vector2 BottomLeft;
    public readonly Vector2 BottomRight;

    public Triangle(Vector2 top, Vector2 bottomLeft, Vector2 bottomRight)
    {
        Top = top;
        BottomLeft = bottomLeft;
        BottomRight = bottomRight;
    }

    public readonly Wall[] GetAsWalls()
    {
        var right = new Line(BottomRight, Top);
        var left = new Line(Top, BottomLeft);
        var bottom = new Line(BottomLeft, BottomRight);

        return new Wall[]
        {
            right.ToWall(),
            left.ToWall(),
            bottom.ToWall()
        };
    }
}
