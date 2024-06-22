using System.Numerics;

namespace Rays._3D;

public sealed class TriangleIntersectorCombinator : ITriangleSetIntersector
{
    private readonly List<ITriangleSetIntersector> _triangleSetIntersectors;

    public TriangleIntersectorCombinator(List<ITriangleSetIntersector> triangleSetIntersectors)
    {
        _triangleSetIntersectors = triangleSetIntersectors;
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
        foreach (var texturedTriangles in _triangleSetIntersectors)
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
        return _triangleSetIntersectors.SelectMany(x => x.GetTriangles());
    }
}
