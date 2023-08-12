using System.Numerics;
using static Rays._3D.Triangle;

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

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) intersection)
    {
        var rayTriangleOptimizedIntersection = new RayTriangleOptimizedIntersection(ray);
        return TryGetIntersection(rayTriangleOptimizedIntersection, out intersection);
    }

    public bool TryGetIntersection(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < Triangles.Length; i++)
        {
            if (!Triangles[i].TryGetIntersection(rayTriangleOptimizedIntersection, out TriangleIntersection triangleIntersection))
            {
                continue;
            }

            float distance = Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, triangleIntersection.GetIntersection(rayTriangleOptimizedIntersection));
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
