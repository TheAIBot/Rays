namespace Rays._3D;

public interface ITexturedTriangles
{
    Triangle[] Triangles { get; }

    Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection);

    ITexturedTriangles SubCopy(Func<Triangle, bool> filter);
}
