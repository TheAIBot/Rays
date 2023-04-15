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
        float thirdAxis = 1.0f - FirstAxisPercent - SecondAxisPercent;
        return cornerAValue * FirstAxisPercent + cornerBValue * SecondAxisPercent + cornerCValue * thirdAxis;
    }
}
