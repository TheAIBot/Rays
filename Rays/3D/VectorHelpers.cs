using System.Numerics;

namespace Rays._3D;

public static class VectorHelpers
{
    public static Vector4 ToZeroExtendedVector4(this Vector3 vector) => new Vector4(vector, 0);

    public static Vector3 ToTruncatedVector3(this Vector4 vector) => new Vector3(vector.X, vector.Y, vector.Z);
}