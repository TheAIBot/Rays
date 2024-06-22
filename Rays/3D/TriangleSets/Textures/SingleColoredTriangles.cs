using System.Numerics;

namespace Rays._3D;

public sealed class SingleColoredTriangles : ISubDividableTriangleSet
{
    public Color TriangleColor;
    public Triangle[] Triangles { get; }

    public SingleColoredTriangles(Triangle[] triangles, Color triangleColor)
    {
        Triangles = triangles;
        TriangleColor = triangleColor;
    }

    public void TryGetIntersections(ReadOnlySpan<Ray> rays, Span<bool> raysHit, Span<(TriangleIntersection intersection, Color color)> triangleIntersections)
    {
        for (int i = 0; i < rays.Length; i++)
        {
            raysHit[i] = TryGetIntersection(rays[i], out triangleIntersections[i]);
        }
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < Triangles.Length; i++)
        {
            if (!Triangles[i].TryGetIntersection(ray, out TriangleIntersection triangleIntersection))
            {
                continue;
            }

            float distance = Vector4.DistanceSquared(ray.Start, triangleIntersection.GetIntersection(ray));
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            intersection.intersection = triangleIntersection;
            intersection.color = TriangleColor;
        }

        return bestDistance != float.MaxValue;
    }

    public ISubDividableTriangleSet SubCopy(Func<Triangle, bool> filter)
    {
        var subTriangles = new List<Triangle>();
        foreach (var triangle in Triangles)
        {
            if (filter(triangle))
            {
                subTriangles.Add(triangle);
            }
        }

        return new SingleColoredTriangles(subTriangles.ToArray(), TriangleColor);
    }

    public ISubDividableTriangleSet SubCopy(IEnumerable<int> triangleIndexes)
    {
        Triangle[] subTriangles = triangleIndexes.Select(x => Triangles[x]).ToArray();
        return new SingleColoredTriangles(subTriangles, TriangleColor);
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return Triangles;
    }
}
