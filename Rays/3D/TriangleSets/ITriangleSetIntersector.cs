using static Rays._3D.Triangle;

namespace Rays._3D;

public interface ITriangleSetIntersector
{
    bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection);
    bool TryGetIntersection(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection, out (TriangleIntersection intersection, Color color) triangleIntersection);
}
