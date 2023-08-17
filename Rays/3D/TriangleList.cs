using System.Numerics;

namespace Rays._3D;

public sealed class TriangleList : ITriangleSetIntersector
{
    private readonly ITriangleSetIntersector[] _texturedTriangles;

    public TriangleList(ITriangleSetIntersector[] texturedTriangles)
    {
        _texturedTriangles = texturedTriangles;
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < _texturedTriangles.Length; i++)
        {
            if (!_texturedTriangles[i].TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
            {
                continue;
            }

            float distance = Vector4.DistanceSquared(ray.Start, triangleIntersection.intersection.GetIntersection(ray));
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            intersection = triangleIntersection;
        }

        return bestDistance != float.MaxValue;
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return _texturedTriangles.SelectMany(x => x.GetTriangles());
    }
}
