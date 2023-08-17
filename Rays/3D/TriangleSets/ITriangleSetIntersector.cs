using System.Numerics;

namespace Rays._3D;

public interface ITriangleSetIntersector
{
    bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection);

    IEnumerable<Triangle> GetTriangles();
}

public sealed class TriangleIntersectorCombinator : ITriangleSetIntersector
{
    private readonly List<ITriangleSetIntersector> _triangleSetIntersectors;

    public TriangleIntersectorCombinator(List<ITriangleSetIntersector> triangleSetIntersectors)
    {
        _triangleSetIntersectors = triangleSetIntersectors;
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
