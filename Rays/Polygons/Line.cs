using System.Numerics;

namespace Rays.Polygons;

public readonly record struct Line(Vector2 Start, Vector2 End)
{
    public readonly Vector2 Direction => End - Start;
}