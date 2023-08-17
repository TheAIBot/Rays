using System.Numerics;
using System.Runtime.Intrinsics.X86;

namespace Rays._3D;

public readonly record struct AxisAlignedBox(Vector4 MinPosition, Vector4 MaxPosition)
{
    public Vector4 Center => MinPosition + ((MaxPosition - MinPosition) * 0.5f);

    public Vector4 Size => Vector4.Abs(MaxPosition - MinPosition);


    public bool Intersects(Ray ray)
    {
        return Intersects(new RayAxisAlignBoxOptimizedIntersection(ray));
    }

    //https://gist.github.com/DomNomNom/46bb1ce47f68d255fd5d
    public bool Intersects(RayAxisAlignBoxOptimizedIntersection optimizedRay)
    {
        if (!Sse.IsSupported)
        {
            return IntersectsCrossPlatform(optimizedRay);
        }

        Vector4 tMin = (MinPosition - optimizedRay.Start) * optimizedRay.InverseDirection;
        Vector4 tMax = (MaxPosition - optimizedRay.Start) * optimizedRay.InverseDirection;
        float tNear = Vector4.Min(tMin, tMax).HorizontalMax();
        float tFar = Vector4.Max(tMin, tMax).HorizontalMin();

        return tNear <= tFar && tFar >= 0;
    }

    //https://gist.github.com/DomNomNom/46bb1ce47f68d255fd5d
    public bool IntersectsCrossPlatform(RayAxisAlignBoxOptimizedIntersection optimizedRay)
    {
        Vector4 tMin = (MinPosition - optimizedRay.Start) * optimizedRay.InverseDirection;
        Vector4 tMax = (MaxPosition - optimizedRay.Start) * optimizedRay.InverseDirection;
        Vector4 t1 = Vector4.Min(tMin, tMax);
        Vector4 t2 = Vector4.Max(tMin, tMax);
        float tNear = float.Max(float.Max(t1.X, t1.Y), t1.Z);
        float tFar = float.Min(float.Min(t2.X, t2.Y), t2.Z);
        return tNear <= tFar && tFar >= 0;
    }

    public bool CollidesWith(Triangle triangle)
    {
        if (CollidesWith(triangle.CornerA) ||
            CollidesWith(triangle.CornerB) ||
            CollidesWith(triangle.CornerC))
        {
            return true;
        }

        return Intersects(triangle);
    }

    private bool CollidesWith(Vector4 point)
    {
        return MinPosition.X <= point.X &&
               MinPosition.Y <= point.Y &&
               MinPosition.Z <= point.Z &&
               MinPosition.W <= point.W &&
               MaxPosition.X >= point.X &&
               MaxPosition.Y >= point.Y &&
               MaxPosition.Z >= point.Z &&
               MaxPosition.W >= point.W;
    }

    public static AxisAlignedBox GetBoundingBoxForTriangles(IEnumerable<Triangle> triangles)
    {
        var min = new Vector4(float.MaxValue);
        var max = new Vector4(float.MinValue);
        foreach (var triangle in triangles)
        {
            min = Vector4.Min(min, triangle.CornerA);
            min = Vector4.Min(min, triangle.CornerB);
            min = Vector4.Min(min, triangle.CornerC);

            max = Vector4.Max(max, triangle.CornerA);
            max = Vector4.Max(max, triangle.CornerB);
            max = Vector4.Max(max, triangle.CornerC);
        }

        return new AxisAlignedBox(min, max);
    }

    public static AxisAlignedBox GetBoundingBoxForBoxes(IEnumerable<AxisAlignedBox> boxes)
    {
        var min = new Vector4(float.MaxValue);
        var max = new Vector4(float.MinValue);
        foreach (var box in boxes)
        {
            min = Vector4.Min(min, box.MinPosition);
            max = Vector4.Max(max, box.MaxPosition);
        }

        return new AxisAlignedBox(min, max);
    }

    //https://gist.github.com/StagPoint/76ae48f5d7ca2f9820748d08e55c9806
    private bool Intersects(Triangle triangle)
    {
        // From the book "Real-Time Collision Detection" by Christer Ericson, page 169
        // See also the published Errata at http://realtimecollisiondetection.net/books/rtcd/errata/

        Vector4 boxExtents = (MaxPosition - MinPosition) * 0.5f;

        // Translate triangle as conceptually moving AABB to origin
        var v0 = (triangle.CornerA - Center);
        var v1 = (triangle.CornerB - Center);
        var v2 = (triangle.CornerC - Center);

        // Compute edge vectors for triangle
        var f0 = (v1 - v0);
        var f1 = (v2 - v1);
        var f2 = (v0 - v2);

        #region Test axes a00..a22 (category 3)

        // Test axis a00
        var a00 = new Vector4(0, -f0.Z, f0.Y, 0);
        var p0 = Vector4.Dot(v0, a00);
        var p1 = Vector4.Dot(v1, a00);
        var p2 = Vector4.Dot(v2, a00);
        var r = boxExtents.Y * Math.Abs(f0.Z) + boxExtents.Z * Math.Abs(f0.Z);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a01
        var a01 = new Vector4(0, -f1.Z, f1.Y, 0);
        p0 = Vector4.Dot(v0, a01);
        p1 = Vector4.Dot(v1, a01);
        p2 = Vector4.Dot(v2, a01);
        r = boxExtents.Y * Math.Abs(f1.Z) + boxExtents.Z * Math.Abs(f1.Y);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a02
        var a02 = new Vector4(0, -f2.Z, f2.Y, 0);
        p0 = Vector4.Dot(v0, a02);
        p1 = Vector4.Dot(v1, a02);
        p2 = Vector4.Dot(v2, a02);
        r = boxExtents.Y * Math.Abs(f2.Z) + boxExtents.Z * Math.Abs(f2.Y);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a10
        var a10 = new Vector4(f0.Z, 0, -f0.X, 0);
        p0 = Vector4.Dot(v0, a10);
        p1 = Vector4.Dot(v1, a10);
        p2 = Vector4.Dot(v2, a10);
        r = boxExtents.X * Math.Abs(f0.Z) + boxExtents.Z * Math.Abs(f0.X);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a11
        var a11 = new Vector4(f1.Z, 0, -f1.X, 0);
        p0 = Vector4.Dot(v0, a11);
        p1 = Vector4.Dot(v1, a11);
        p2 = Vector4.Dot(v2, a11);
        r = boxExtents.X * Math.Abs(f1.Z) + boxExtents.Z * Math.Abs(f1.X);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a12
        var a12 = new Vector4(f2.Z, 0, -f2.X, 0);
        p0 = Vector4.Dot(v0, a12);
        p1 = Vector4.Dot(v1, a12);
        p2 = Vector4.Dot(v2, a12);
        r = boxExtents.X * Math.Abs(f2.Z) + boxExtents.Z * Math.Abs(f2.X);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a20
        var a20 = new Vector4(-f0.Y, f0.X, 0, 0);
        p0 = Vector4.Dot(v0, a20);
        p1 = Vector4.Dot(v1, a20);
        p2 = Vector4.Dot(v2, a20);
        r = boxExtents.X * Math.Abs(f0.Y) + boxExtents.Y * Math.Abs(f0.X);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a21
        var a21 = new Vector4(-f1.Y, f1.X, 0, 0);
        p0 = Vector4.Dot(v0, a21);
        p1 = Vector4.Dot(v1, a21);
        p2 = Vector4.Dot(v2, a21);
        r = boxExtents.X * Math.Abs(f1.Y) + boxExtents.Y * Math.Abs(f1.X);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a22
        var a22 = new Vector4(-f2.Y, f2.X, 0, 0);
        p0 = Vector4.Dot(v0, a22);
        p1 = Vector4.Dot(v1, a22);
        p2 = Vector4.Dot(v2, a22);
        r = boxExtents.X * Math.Abs(f2.Y) + boxExtents.Y * Math.Abs(f2.X);
        if (Math.Max(-Max(p0, p1, p2), Min(p0, p1, p2)) > r)
        {
            return false;
        }

        #endregion

        #region Test the three axes corresponding to the face normals of AABB b (category 1)

        // Exit if...
        // ... [-extents.x, extents.x] and [min(v0.x,v1.x,v2.x), max(v0.x,v1.x,v2.x)] do not overlap
        if (Max(v0.X, v1.X, v2.X) < -boxExtents.X || Min(v0.X, v1.X, v2.X) > boxExtents.X)
        {
            return false;
        }

        // ... [-extents.y, extents.y] and [min(v0.y,v1.y,v2.y), max(v0.y,v1.y,v2.y)] do not overlap
        if (Max(v0.Y, v1.Y, v2.Y) < -boxExtents.Y || Min(v0.Y, v1.Y, v2.Y) > boxExtents.Y)
        {
            return false;
        }

        // ... [-extents.z, extents.z] and [min(v0.z,v1.z,v2.z), max(v0.z,v1.z,v2.z)] do not overlap
        if (Max(v0.Z, v1.Z, v2.Z) < -boxExtents.Z || Min(v0.Z, v1.Z, v2.Z) > boxExtents.Z)
        {
            return false;
        }

        #endregion

        #region Test separating axis corresponding to triangle face normal (category 2)

        var planeNormal = Vector3.Cross(f0.ToTruncatedVector3(), f1.ToTruncatedVector3());
        var planeDistance = Vector4.Dot(planeNormal.ToZeroExtendedVector4(), v0);

        // Compute the projection interval radius of b onto L(t) = b.c + t * p.n
        r = boxExtents.X * Math.Abs(planeNormal.X)
            + boxExtents.Y * Math.Abs(planeNormal.Y)
            + boxExtents.Z * Math.Abs(planeNormal.Z);

        // Intersection occurs when plane distance falls within [-r,+r] interval
        if (planeDistance > r)
        {
            return false;
        }

        #endregion

        return true;
    }

    private static float Min(float a, float b, float c)
    {
        return Math.Min(a, Math.Min(b, c));
    }

    private static float Max(float a, float b, float c)
    {
        return Math.Max(a, Math.Max(b, c));
    }

    public readonly record struct RayAxisAlignBoxOptimizedIntersection(Vector4 Start, Vector4 InverseDirection)
    {
        public RayAxisAlignBoxOptimizedIntersection(Ray ray) : this(ray.Start, Vector4.One / ray.Direction) { }
    }
}