using System.Numerics;
using static Rays._3D.Triangle;

namespace Rays._3D;

public readonly record struct TriangleIntersection(float RayDirectionMultiplier, float FirstAxisPercent, float SecondAxisPercent)
{
    public Vector4 GetIntersection(Ray ray)
    {
        return ray.Start + RayDirectionMultiplier * ray.Direction;
    }

    public Vector4 GetIntersection(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection)
    {
        return rayTriangleOptimizedIntersection.Start + RayDirectionMultiplier * rayTriangleOptimizedIntersection.Direction;
    }

    public Vector3 Interpolate(Vector3 cornerAValue, Vector3 cornerBValue, Vector3 cornerCValue)
    {
        float thirdAxis = 1.0f - FirstAxisPercent - SecondAxisPercent;
        return cornerAValue * FirstAxisPercent + cornerBValue * SecondAxisPercent + cornerCValue * thirdAxis;
    }
}
