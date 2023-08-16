using System.Numerics;
using static Rays._3D.Triangle;

namespace Rays._3D;

public sealed class SimpleColoredTriangles : ISubDividableTriangleSet
{
    private readonly Triangle _triangleColor;
    public Triangle[] Triangles { get; }

    public SimpleColoredTriangles(Triangle[] triangles, Triangle triangleColor)
    {
        Triangles = triangles;
        _triangleColor = triangleColor;
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
            intersection.color = GetTriangleIntersectionTextureColor(triangleIntersection);
        }

        return bestDistance != float.MaxValue;
    }

    private Color GetTriangleIntersectionTextureColor(TriangleIntersection triangleIntersection)
    {
        Vector4 color = triangleIntersection.Interpolate(_triangleColor.CornerA, _triangleColor.CornerB, _triangleColor.CornerC);
        return new Color((int)color.X, (int)color.Y, (int)color.Z, 255);
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

        return new SimpleColoredTriangles(subTriangles.ToArray(), _triangleColor);
    }

    public ISubDividableTriangleSet SubCopy(IEnumerable<int> triangleIndexes)
    {
        Triangle[] subTriangles = triangleIndexes.Select(x => Triangles[x]).ToArray();
        return new SimpleColoredTriangles(subTriangles, _triangleColor);
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return Triangles;
    }
}
