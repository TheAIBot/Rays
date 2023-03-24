using System.Numerics;

namespace Rays.Polygons;

public readonly record struct Rectangle(Vector2 BottomLeft, Vector2 Horizontal, Vector2 Vertical)
{
    public readonly Vector2 TopLeft => BottomLeft + Vertical;
    public readonly Vector2 TopRight => BottomLeft + Vertical + Horizontal;
    public readonly Vector2 BottomRight => BottomLeft + Horizontal;
    public readonly Vector2 Center => BottomLeft + ((Vertical + Horizontal) / 2);

    public readonly Wall[] GetAsWalls()
    {
        var top = new Line(TopLeft, TopRight);
        var bottom = new Line(BottomRight, BottomLeft);
        var left = new Line(BottomLeft, TopLeft);
        var right = new Line(TopRight, BottomRight);

        return new Wall[]
        {
            top.ToWall(),
            bottom.ToWall(),
            left.ToWall(),
            right.ToWall()
        };
    }

    public static Vector2 Rotate90DegCounterClockwise(Vector2 point)
    {
        return new Vector2(-point.Y, point.X);
    }

    public static Rectangle RotateRectangle(Rectangle rectangle, float angleInDeg)
    {
        float angle = DegreesToRadians(angleInDeg);
        Matrix3x2 rotator = Matrix3x2.CreateRotation(angle);
        Vector2 toCenter = (rectangle.Horizontal + rectangle.Vertical) / 2;
        Vector2 centerPosition = rectangle.BottomLeft + toCenter;
        Vector2 gre = Vector2.Transform(toCenter, rotator);
        Vector2 newPos = centerPosition - gre;

        return new Rectangle(newPos, Vector2.Transform(rectangle.Horizontal, rotator), Vector2.Transform(rectangle.Vertical, rotator));
    }

    private static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180);
    }
}
