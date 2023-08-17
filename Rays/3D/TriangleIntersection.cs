using System.Numerics;

namespace Rays._3D;

public readonly record struct TriangleIntersection(float RayDirectionMultiplier, float FirstAxisPercent, float SecondAxisPercent)
{
    public Vector4 GetIntersection(Ray ray)
    {
        return ray.Start + RayDirectionMultiplier * ray.Direction;
    }

    public Vector4 Interpolate(Vector4 cornerAValue, Vector4 cornerBValue, Vector4 cornerCValue)
    {
        float thirdAxis = 1.0f - FirstAxisPercent - SecondAxisPercent;
        return cornerAValue * FirstAxisPercent + cornerBValue * SecondAxisPercent + cornerCValue * thirdAxis;
    }
}
