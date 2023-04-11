namespace Rays.GeometryLoaders.Geometry;

public readonly record struct Triangle<T>(T CornerA, T CornerB, T CornerC) where T : struct
{
    internal const int EdgeCount = 3;
}