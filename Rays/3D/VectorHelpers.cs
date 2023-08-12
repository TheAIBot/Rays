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
        Vector128<float> vector128 = vector.AsVector128();
        Vector128<float> min = Sse.Min(Sse.Min(Sse.MoveHighToLow(vector128, vector128),
                                               Sse.Shuffle(vector128, vector128, 0b00_00_11_01)),
                                       vector128);
        return min.GetElement(0);
    }

    public static float HorizontalMax(this Vector4 vector)
    {
        Vector128<float> vector128 = vector.AsVector128();
        Vector128<float> max = Sse.Max(Sse.Max(Sse.MoveHighToLow(vector128, vector128),
                                               Sse.Shuffle(vector128, vector128, 0b00_00_11_01)),
                                       vector128);
        return max.GetElement(0);
    }
}