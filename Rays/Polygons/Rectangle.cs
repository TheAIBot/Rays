using System.Numerics;

namespace Rays.Polygons;

public readonly struct Rectangle
{
    public readonly Vector2 BottomLeft;
    public readonly Vector2 Horizontal;
    public readonly Vector2 Vertical;
    public readonly Vector2 TopLeft => BottomLeft + Vertical;
    public readonly Vector2 TopRight => BottomLeft + Vertical + Horizontal;
    public readonly Vector2 BottomRight => BottomLeft + Horizontal;
    public readonly Vector2 Center => BottomLeft + ((Vertical + Horizontal) / 2);

    public Rectangle(Vector2 bottomLeft, Vector2 horizontal, Vector2 vertical)
    {
        BottomLeft = bottomLeft;
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public readonly Wall[] GetAsWalls()
    {
        var top = new Line(TopLeft, TopRight);
        var bottom = new Line(BottomRight, BottomLeft);
        var left = new Line(BottomLeft, TopLeft);
        var right = new Line(TopRight, BottomRight);

        return new Wall[]
        {
            new Wall(top, Vector2.Normalize(Rotate90DegCounterClockwise(top.Direction))),
            new Wall(bottom, Vector2.Normalize(Rotate90DegCounterClockwise(bottom.Direction))),
            new Wall(left, Vector2.Normalize(Rotate90DegCounterClockwise(left.Direction))),
            new Wall(right, Vector2.Normalize(Rotate90DegCounterClockwise(right.Direction)))
        };
    }

    public static Vector2 Rotate90DegCounterClockwise(Vector2 point)
    {
        return new Vector2(-point.Y, point.X);
    }
}
