using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Rays._3D;

public readonly record struct Triangle(Vector3 CornerA, Vector3 CornerB, Vector3 CornerC)
{
    public bool TryGetIntersection(Ray ray, out TriangleIntersection intersection)
    {
        return TryGetIntersection(new RayTriangleOptimizedIntersection(ray), out intersection);
    }

    // https://stackoverflow.com/a/42752998

    public bool TryGetIntersection(RayTriangleOptimizedIntersection optimizedRay, out TriangleIntersection intersection)
    {
        if (!Sse41.IsSupported)
        {
            return TryGetIntersectionCrossPlatform(optimizedRay, out intersection);
        }

        var a = CornerA.AsVector128();
        var b = CornerB.AsVector128();
        var c = CornerC.AsVector128();
        var rayStart = optimizedRay.Start.AsVector128();
        var rayDirection = optimizedRay.Direction.AsVector128();
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
    private bool TryGetIntersectionCrossPlatform(RayTriangleOptimizedIntersection optimizedRay, out TriangleIntersection intersection)
    {
        var rayStart = new Vector3(optimizedRay.Start.X, optimizedRay.Start.Y, optimizedRay.Start.Z);
        var rayDirection = new Vector3(optimizedRay.Direction.X, optimizedRay.Direction.Y, optimizedRay.Direction.Z);
        Vector3 E1 = CornerB - CornerA;
        Vector3 E2 = CornerC - CornerA;
        Vector3 N = Vector3.Cross(E1, E2);
        float det = -Vector3.Dot(rayDirection, N);
        if (det < 1e-6f)
        {
            intersection = default;
            return false;
        }

        float invdet = 1.0f / det;
        Vector3 AO = rayStart - CornerA;
        Vector3 DAO = Vector3.Cross(AO, rayDirection);
        float u = Vector3.Dot(E2, DAO) * invdet;
        float v = -Vector3.Dot(E1, DAO) * invdet;
        float t = Vector3.Dot(AO, N) * invdet;

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

    public readonly record struct RayTriangleOptimizedIntersection(Vector4 Start, Vector4 Direction)
    {
        public RayTriangleOptimizedIntersection(Ray ray) : this(new Vector4(ray.Start, 0), new Vector4(ray.Direction, 0)) { }
    }
}