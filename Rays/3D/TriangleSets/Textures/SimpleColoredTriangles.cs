﻿using System.Numerics;

namespace Rays._3D;

public sealed class SimpleColoredTriangles : ISubDividableTriangleSet
{
    private readonly Triangle _triangleColor;
    private readonly AxisAlignedBox _boundingBox;
    public Triangle[] Triangles { get; }

    public SimpleColoredTriangles(Triangle[] triangles, Triangle triangleColor)
    {
        Triangles = triangles;
        _triangleColor = triangleColor;
        _boundingBox = AxisAlignedBox.GetBoundingBoxForTriangles(triangles);
    }

    public AxisAlignedBox GetBoundingBox() => _boundingBox;
    public void OptimizeIntersectionFromSceneInformation(Vector4 cameraPosition, Frustum frustum) { }

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
