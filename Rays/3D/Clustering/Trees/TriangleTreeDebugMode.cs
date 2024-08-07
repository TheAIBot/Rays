﻿using System.Numerics;

namespace Rays._3D;

public sealed class TriangleTreeDebugMode : ITriangleSetIntersector
{
    private readonly TriangleTree[] _nodeLevels;
    private readonly AxisAlignedBox _boundingBox;

    public bool IsEnabled => DisplayLevel != 0;
    public int DisplayLevel { get; private set; } = 0;

    public int MaxDisplayLevel => _nodeLevels.Length;

    public TriangleTreeDebugMode(TriangleTree[] nodeLevels)
    {
        _nodeLevels = nodeLevels;
        _boundingBox = AxisAlignedBox.GetBoundingBoxForBoxes(_nodeLevels.Select(x => x.GetBoundingBox()));
    }

    public AxisAlignedBox GetBoundingBox() => _boundingBox;
    public void OptimizeIntersectionFromSceneInformation(Vector4 cameraPosition, Frustum frustum) { }

    public void ChangeDisplayLevel(int level)
    {
        if (level < 0 || level > _nodeLevels.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(level), $"Must be within 0 to {_nodeLevels.Length} but was {level}.");
        }

        DisplayLevel = level;
    }

    public void TryGetIntersections(ReadOnlySpan<Ray> rays, Span<bool> raysHit, Span<(TriangleIntersection intersection, Color color)> triangleIntersections)
    {
        for (int i = 0; i < rays.Length; i++)
        {
            raysHit[i] = TryGetIntersection(rays[i], out triangleIntersections[i]);
        }
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        if (DisplayLevel == 0)
        {
            triangleIntersection = default;
            return false;
        }

        return _nodeLevels[DisplayLevel - 1].TryGetIntersection(ray, out triangleIntersection);
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return _nodeLevels.SelectMany(x => x.GetTriangles());
    }
}
