﻿using System.Numerics;
using static Rays._3D.Triangle;

namespace Rays._3D;

public sealed class TexturedTriangles : ISubDividableTriangleSet
{
    private readonly Triangle[] _textureTriangles;
    private readonly Image<Rgba32> _texture;
    public Triangle[] Triangles { get; }

    public TexturedTriangles(Triangle[] triangles, Triangle[] textureTriangles, Image<Rgba32> texture)
    {
        Triangles = triangles;
        _textureTriangles = textureTriangles;
        _texture = texture;
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
            intersection.color = GetTriangleIntersectionTextureColor(i, triangleIntersection);
        }

        return bestDistance != float.MaxValue;
    }

    private Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection)
    {
        var triangleTextureCoordinates = _textureTriangles[triangleIndex];
        Vector3 interpolatedTextureCoordinate = triangleIntersection.Interpolate(triangleTextureCoordinates.CornerA,
                                                                                 triangleTextureCoordinates.CornerB,
                                                                                 triangleTextureCoordinates.CornerC);
        int texturePositionX = (int)(interpolatedTextureCoordinate.X * _texture.Width);
        int texturePositionY = (int)(interpolatedTextureCoordinate.Y * _texture.Height);

        Rgba32 textureColor = _texture[texturePositionX, texturePositionY];
        return new Color(textureColor.R, textureColor.G, textureColor.B, textureColor.A);
    }

    public ISubDividableTriangleSet SubCopy(Func<Triangle, bool> filter)
    {
        List<Triangle> subTriangles = new List<Triangle>();
        List<Triangle> subTextureTriangles = new List<Triangle>();
        for (int i = 0; i < Triangles.Length; i++)
        {
            if (filter(Triangles[i]))
            {
                subTriangles.Add(Triangles[i]);
                subTextureTriangles.Add(_textureTriangles[i]);
            }
        }

        return new TexturedTriangles(subTriangles.ToArray(), subTextureTriangles.ToArray(), _texture);
    }
}