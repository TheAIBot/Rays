using System.Numerics;

namespace Rays._3D;

public readonly record struct TriangleIntersection(float RayDirectionMultiplier, float FirstAxisPercent, float SecondAxisPercent)
{
    public Vector3 GetIntersection(Ray ray)
    {
        return ray.Start + RayDirectionMultiplier * ray.Direction;
    }

    public Vector3 Interpolate(Vector3 cornerAValue, Vector3 cornerBValue, Vector3 cornerCValue)
    {
        return (1 - FirstAxisPercent) * cornerAValue * 0.5f +
               FirstAxisPercent * cornerBValue +
               (1 - SecondAxisPercent) * cornerAValue * 0.5f +
               SecondAxisPercent * cornerCValue;
    }
}
