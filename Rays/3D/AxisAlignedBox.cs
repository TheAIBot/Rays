using System.Numerics;

namespace Rays._3D;

public readonly record struct AxisAlignedBox(Vector3 MinPosition, Vector3 MaxPosition)
{
    //https://gist.github.com/DomNomNom/46bb1ce47f68d255fd5d
    public bool Intersects(Ray ray)
    {
        Vector3 tMin = (MinPosition - ray.Start) / ray.Direction;
        Vector3 tMax = (MaxPosition - ray.Start) / ray.Direction;
        Vector3 t1 = Vector3.Min(tMin, tMax);
        Vector3 t2 = Vector3.Max(tMin, tMax);
        float tNear = float.Max(float.Max(t1.X, t1.Y), t1.Z);
        float tFar = float.Min(float.Min(t2.X, t2.Y), t2.Z);
        return tNear <= tFar && tFar >= 0;
    }
}
