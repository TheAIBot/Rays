using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Rays._3D;

public static class VectorHelpers
{
    public static Vector4 ToZeroExtendedVector4(this Vector3 vector) => new(vector, 0);

    public static Vector3 ToTruncatedVector3(this Vector4 vector) => new(vector.X, vector.Y, vector.Z);

    public static float HorizontalMin(this Vector4 vector)
    {
        if (!Sse.IsSupported)
        {
            return MathF.Min(MathF.Min(vector.X, vector.Y), MathF.Min(vector.Z, vector.W));
        }

        Vector128<float> vector128 = vector.AsVector128();
        Vector128<float> min = Sse.Min(Sse.Min(Sse.MoveHighToLow(vector128, vector128),
                                               Sse.Shuffle(vector128, vector128, 0b00_00_11_01)),
                                       vector128);
        return min.GetElement(0);
    }

    public static float HorizontalMax(this Vector4 vector)
    {
        if (!Sse.IsSupported)
        {
            return MathF.Max(MathF.Max(vector.X, vector.Y), MathF.Max(vector.Z, vector.W));
        }

        Vector128<float> vector128 = vector.AsVector128();
        Vector128<float> max = Sse.Max(Sse.Max(Sse.MoveHighToLow(vector128, vector128),
                                               Sse.Shuffle(vector128, vector128, 0b00_00_11_01)),
                                       vector128);
        return max.GetElement(0);
    }

    public static Vector4 Cross(this Vector4 vector1, Vector4 vector2)
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