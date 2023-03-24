using System.Numerics;

namespace Rays.Polygons;

public readonly record struct Line(Vector2 Start, Vector2 End)
{
    public readonly Vector2 Direction => End - Start;

    public readonly Wall ToWall()
    {
        return new Wall(this, Vector2.Normalize(Rotate90DegCounterClockwise(Direction)));
    }

    private static Vector2 Rotate90DegCounterClockwise(Vector2 point)
    {
        return new Vector2(-point.Y, point.X);
    }
}