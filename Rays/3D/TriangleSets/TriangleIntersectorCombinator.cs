using System.Numerics;

namespace Rays._3D;

public sealed class TriangleIntersectorCombinator : ITriangleSetIntersector
{
    private readonly ITriangleSetIntersector[] _allTriangleIntersectors;
    private readonly ITriangleSetIntersector[] _intersectionOptimizedTriangleIntersectorsBuffer;
    private Memory<ITriangleSetIntersector> _intersectionOptimizedTriangleIntersectors;
    private readonly AxisAlignedBox _boundingBox;

    public TriangleIntersectorCombinator(List<ITriangleSetIntersector> triangleSetIntersectors)
    {
        _allTriangleIntersectors = triangleSetIntersectors.ToArray();
        _intersectionOptimizedTriangleIntersectorsBuffer = new ITriangleSetIntersector[_allTriangleIntersectors.Length];
        _intersectionOptimizedTriangleIntersectors = _intersectionOptimizedTriangleIntersectorsBuffer;
        Array.Copy(_allTriangleIntersectors, _intersectionOptimizedTriangleIntersectorsBuffer, _allTriangleIntersectors.Length);
        _boundingBox = AxisAlignedBox.GetBoundingBoxForBoxes(_allTriangleIntersectors.Select(x => x.GetBoundingBox()));
    }

    public AxisAlignedBox GetBoundingBox() => _boundingBox;
    public void OptimizeIntersectionFromSceneInformation(Vector4 cameraPosition, Frustum frustum)
    {
        var orderIntersectors = new PriorityQueue<ITriangleSetIntersector, float>();
        foreach (var intersector in _allTriangleIntersectors)
        {
            AxisAlignedBox intersectorBoundingBox = intersector.GetBoundingBox();
            if (!frustum.Intersects(intersectorBoundingBox))
            {
                continue;
            }

            orderIntersectors.Enqueue(intersector, intersector.GetBoundingBox().GetDistance(cameraPosition));
        }

        int intersectorsInView = orderIntersectors.Count;
        for (int i = 0; i < orderIntersectors.Count; i++)
        {
            _intersectionOptimizedTriangleIntersectorsBuffer[i] = orderIntersectors.Dequeue();
        }

        _intersectionOptimizedTriangleIntersectors = _intersectionOptimizedTriangleIntersectorsBuffer.AsMemory(0, intersectorsInView);

        Span<ITriangleSetIntersector> triangleIntersectors = _intersectionOptimizedTriangleIntersectors.Span;
        for (int i = 0; i < triangleIntersectors.Length; i++)
        {
            triangleIntersectors[i].OptimizeIntersectionFromSceneInformation(cameraPosition, frustum);
        }
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
        triangleIntersection = default;
        float bestDistance = float.MaxValue;
        Span<ITriangleSetIntersector> triangleIntersectors = _intersectionOptimizedTriangleIntersectors.Span;
        foreach (var texturedTriangles in triangleIntersectors)
        {
            if (!texturedTriangles.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) intersection))
            {
                continue;
            }

            float distance = Vector4.DistanceSquared(ray.Start, intersection.intersection.GetIntersection(ray));
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            triangleIntersection = intersection;
        }

        return bestDistance != float.MaxValue;
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return _allTriangleIntersectors.SelectMany(x => x.GetTriangles());
    }
}
