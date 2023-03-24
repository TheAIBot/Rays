using System.Numerics;

namespace Rays.Polygons;

public readonly record struct Triangle(Vector2 Top, Vector2 BottomLeft, Vector2 BottomRight)
{
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
