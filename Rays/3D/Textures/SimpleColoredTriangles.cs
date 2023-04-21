using System.Numerics;

namespace Rays._3D;

public sealed class SimpleColoredTriangles : ITexturedTriangles
{
    private readonly Triangle _triangleColor;
    public Triangle[] Triangles { get; }

    public SimpleColoredTriangles(Triangle[] triangles, Triangle triangleColor)
    {
        Triangles = triangles;
        _triangleColor = triangleColor;
    }

    public Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection)
    {
        Vector3 color = triangleIntersection.Interpolate(_triangleColor.CornerA, _triangleColor.CornerB, _triangleColor.CornerC);
        return new Color((int)color.X, (int)color.Y, (int)color.Z, 255);
    }

    public ITexturedTriangles SubCopy(Func<Triangle, bool> filter)
    {
        List<Triangle> subTriangles = new List<Triangle>();
        foreach (var triangle in Triangles)
        {
            if (filter(triangle))
            {
                subTriangles.Add(triangle);
            }
        }

        return new SimpleColoredTriangles(subTriangles.ToArray(), _triangleColor);
    }
}
