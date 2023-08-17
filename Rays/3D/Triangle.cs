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
        Vector4 E1 = CornerB - CornerA;
        Vector4 E2 = CornerC - CornerA;
        Vector4 N = Cross(E1, E2);
        float det = -Vector4.Dot(ray.Direction, N);
        if (det < 1e-6f)
        {
            intersection = default;
            return false;
        }

        Vector4 AO = ray.Start - CornerA;
        Vector4 DAO = Cross(AO, ray.Direction);
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

    private static Vector4 Cross(Vector4 vector1, Vector4 vector2)
    {
        if (Sse.IsSupported)
        {
            return Cross(vector1.AsVector128(), vector2.AsVector128()).AsVector4();
        }

        return Vector3.Cross(vector1.ToTruncatedVector3(), vector2.ToTruncatedVector3()).ToZeroExtendedVector4();
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