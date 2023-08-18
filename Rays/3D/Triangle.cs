using System.Numerics;

namespace Rays._3D;

public readonly record struct Triangle(Vector4 CornerA, Vector4 CornerB, Vector4 CornerC)
{
    public Vector4 Center => (CornerA + CornerB + CornerC) / 3.0f;

    public Triangle(Vector3 a, Vector3 b, Vector3 c) : this(a.ToZeroExtendedVector4(), b.ToZeroExtendedVector4(), c.ToZeroExtendedVector4())
    { }

    // https://stackoverflow.com/a/42752998

    public bool TryGetIntersection(Ray ray, out TriangleIntersection intersection)
    {
        Vector4 E1 = CornerB - CornerA;
        Vector4 E2 = CornerC - CornerA;
        Vector4 N = E1.Cross(E2);
        float det = -Vector4.Dot(ray.Direction, N);
        if (det < 1e-6f)
        {
            intersection = default;
            return false;
        }

        Vector4 AO = ray.Start - CornerA;
        Vector4 DAO = AO.Cross(ray.Direction);
        float u = Vector4.Dot(E2, DAO);
        float v = -Vector4.Dot(E1, DAO);
        float t = Vector4.Dot(AO, N);
        if (t < 0.0f ||
            u < 0.0f ||
            v < 0.0f)
        {
            intersection = default;
            return false;
        }

        float inversedet = 1.0f / det;
        t *= inversedet;
        u *= inversedet;
        v *= inversedet;
        intersection = new TriangleIntersection(t, u, v);
        return (u + v) <= 1.0f;
    }
}