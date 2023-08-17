namespace Rays._3D;

public interface ITriangleSetIntersector
{
    bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection);

    IEnumerable<Triangle> GetTriangles();
}