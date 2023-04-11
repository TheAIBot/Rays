using System.Numerics;

namespace Rays._3D;

public readonly record struct Triangle(Vector3 CornerA, Vector3 CornerB, Vector3 CornerC)
{
    // https://stackoverflow.com/a/42752998
    public bool TryGetIntersection(Ray ray, out Vector3 intersection)
    {
        Vector3 E1 = CornerB - CornerA;
        Vector3 E2 = CornerC - CornerA;
        Vector3 N = Vector3.Cross(E1, E2);
        float det = -Vector3.Dot(ray.Direction, N);
        float invdet = 1.0f / det;
        Vector3 AO = ray.Start - CornerA;
        Vector3 DAO = Vector3.Cross(AO, ray.Direction);
        float u = Vector3.Dot(E2, DAO) * invdet;
        float v = -Vector3.Dot(E1, DAO) * invdet;
        float t = Vector3.Dot(AO, N) * invdet;

        intersection = ray.Start + t * ray.Direction;
        return (float.Abs(det) >= 1e-6f && // changed to float.Abs(det) since det did not work
                t >= 0.0 &&
                u >= 0.0 &&
                v >= 0.0 &&
                (u + v) <= 1.0);
    }
}
