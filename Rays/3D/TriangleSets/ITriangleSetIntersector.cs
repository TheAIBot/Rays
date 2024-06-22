namespace Rays._3D;

public interface ITriangleSetIntersector
{
    void TryGetIntersections(ReadOnlySpan<Ray> rays, Span<bool> raysHit, Span<(TriangleIntersection intersection, Color color)> triangleIntersections);
    bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection);

    IEnumerable<Triangle> GetTriangles();
}