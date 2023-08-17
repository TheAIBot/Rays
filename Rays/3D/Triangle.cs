using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Rays._3D;

public readonly record struct Triangle(Vector4 CornerA, Vector4 CornerB, Vector4 CornerC)
{
    public Vector4 Center => (CornerA + CornerB + CornerC) / 3.0f;

    public Triangle(Vector3 a, Vector3 b, Vector3 c) : this(a.ToZeroExtendedVector4(), b.ToZeroExtendedVector4(), c.ToZeroExtendedVector4())
    { }

    // https://stackoverflow.com/a/42752998

    public bool TryGetIntersection(Ray ray, out TriangleIntersection intersection)
    {
        if (!Sse41.IsSupported)
        {
            return TryGetIntersectionCrossPlatform(ray, out intersection);
        }

        var a = CornerA.AsVector128();
        var b = CornerB.AsVector128();
        var c = CornerC.AsVector128();
        var rayStart = ray.Start.AsVector128();
        var rayDirection = ray.Direction.AsVector128();
        Vector128<float> E1 = b - a;
        Vector128<float> E2 = c - a;
        Vector128<float> N = Cross(E1, E2);
        float det = (-Sse41.DotProduct(rayDirection, N, 0b0111_0111)).GetElement(0);
        if (det < 1e-6f)
        {
            intersection = default;
            return false;
        }

        Vector128<float> AO = Sse.Subtract(rayStart, a);
        Vector128<float> DAO = Cross(AO, rayDirection);
        float u = (Sse41.DotProduct(E2, DAO, 0b0111_0111).GetElement(0));
        float v = -(Sse41.DotProduct(E1, DAO, 0b0111_0111).GetElement(0));
        float t = (Sse41.DotProduct(AO, N, 0b0111_0111).GetElement(0));
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
        return ((u + v) <= 1.0f);
    }

    // https://stackoverflow.com/a/42752998
    private bool TryGetIntersectionCrossPlatform(Ray ray, out TriangleIntersection intersection)
    {
        Vector4 E1 = CornerB - CornerA;
        Vector4 E2 = CornerC - CornerA;
        Vector4 N = Vector3.Cross(E1.ToTruncatedVector3(), E2.ToTruncatedVector3()).ToZeroExtendedVector4();
        float det = -Vector4.Dot(ray.Direction, N);
        if (det < 1e-6f)
        {
            intersection = default;
            return false;
        }

        float invdet = 1.0f / det;
        Vector4 AO = ray.Start - CornerA;
        Vector4 DAO = Vector3.Cross(AO.ToTruncatedVector3(), ray.Direction.ToTruncatedVector3()).ToZeroExtendedVector4();
        float u = Vector4.Dot(E2, DAO) * invdet;
        float v = -Vector4.Dot(E1, DAO) * invdet;
        float t = Vector4.Dot(AO, N) * invdet;

        intersection = new TriangleIntersection(t, u, v);
        return (t >= 0.0 &&
                u >= 0.0 &&
                v >= 0.0 &&
                (u + v) <= 1.0);
    }

    //Method 5 from https://geometrian.com/programming/tutorials/cross-product/index.php
    private static Vector128<float> Cross(Vector128<float> vector1, Vector128<float> vector2)
    {
        var tmp0 = Sse.Shuffle(vector1, vector1, 0b11_00_10_01);
        var tmp1 = Sse.Shuffle(vector2, vector2, 0b11_01_00_10);
        var tmp2 = Sse.Multiply(tmp0, vector2);
        var tmp3 = Sse.Multiply(tmp0, tmp1);
        var tmp4 = Sse.Shuffle(tmp2, tmp2, 0b11_00_10_01);
        return Sse.Subtract(tmp3, tmp4);
    }
}