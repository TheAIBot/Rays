﻿using static Rays._3D.Triangle;

namespace Rays._3D;

public sealed class TriangleTreeDebugMode : ITriangleSetIntersector
{
    private readonly TriangleTree[] _nodeLevels;

    public bool IsEnabled => DisplayLevel != 0;
    public int DisplayLevel { get; private set; } = 2;

    public TriangleTreeDebugMode(TriangleTree[] nodeLevels)
    {
        _nodeLevels = nodeLevels;
    }

    public void ChangeDisplayLevel(int level)
    {
        if (level < 0 || level > _nodeLevels.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(level), $"Must be within 0 to {_nodeLevels.Length} but was {level}.");
        }

        DisplayLevel = level;
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        //DisplayLevel = ((DisplayLevel + 1) % _nodeLevels.Length) + 1;
        var rayTriangleOptimizedIntersection = new RayTriangleOptimizedIntersection(ray);
        return TryGetIntersection(rayTriangleOptimizedIntersection, out triangleIntersection);
    }

    public bool TryGetIntersection(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        if (DisplayLevel == 0)
        {
            triangleIntersection = default;
            return false;
        }

        return _nodeLevels[DisplayLevel - 1].TryGetIntersection(rayTriangleOptimizedIntersection, out triangleIntersection);
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return _nodeLevels.SelectMany(x => x.GetTriangles());
    }
}