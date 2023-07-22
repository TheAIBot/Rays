using System.Numerics;
using static Rays._3D.Triangle;

namespace Rays._3D;

public readonly record struct AxisAlignedBox(Vector3 MinPosition, Vector3 MaxPosition)
{
    public Vector3 Center => MinPosition + ((MaxPosition - MinPosition) * 0.5f);

    public Vector3 Size => Vector3.Abs(MaxPosition - MinPosition);


    public bool Intersects(Ray ray)
    {
        return Intersects(new RayAxisAlignBoxOptimizedIntersection(ray));
    }

    //https://gist.github.com/DomNomNom/46bb1ce47f68d255fd5d
    public bool Intersects(RayAxisAlignBoxOptimizedIntersection optimizedRay)
    {
        Vector3 tMin = (MinPosition - optimizedRay.Start) * optimizedRay.InverseDirection;
        Vector3 tMax = (MaxPosition - optimizedRay.Start) * optimizedRay.InverseDirection;
        Vector3 t1 = Vector3.Min(tMin, tMax);
        Vector3 t2 = Vector3.Max(tMin, tMax);
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

    private bool CollidesWith(Vector3 point)
    {
        return MinPosition.X <= point.X &&
               MinPosition.Y <= point.Y &&
               MinPosition.Z <= point.Z &&
               MaxPosition.X >= point.X &&
               MaxPosition.Y >= point.Y &&
               MaxPosition.Z >= point.Z;
    }

    public static AxisAlignedBox GetBoundingBoxForTriangles(IEnumerable<Triangle> triangles)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var triangle in triangles)
        {
            min = Vector3.Min(min, triangle.CornerA);
            min = Vector3.Min(min, triangle.CornerB);
            min = Vector3.Min(min, triangle.CornerC);

            max = Vector3.Max(max, triangle.CornerA);
            max = Vector3.Max(max, triangle.CornerB);
            max = Vector3.Max(max, triangle.CornerC);
        }

        return new AxisAlignedBox(min, max);
    }

    //https://gist.github.com/StagPoint/76ae48f5d7ca2f9820748d08e55c9806
    private bool Intersects(Triangle triangle)
    {
        // From the book "Real-Time Collision Detection" by Christer Ericson, page 169
        // See also the published Errata at http://realtimecollisiondetection.net/books/rtcd/errata/

        Vector3 boxExtents = (MaxPosition - MinPosition) * 0.5f;

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
        var a00 = new Vector3(0, -f0.Z, f0.Y);
        var p0 = Vector3.Dot(v0, a00);
        var p1 = Vector3.Dot(v1, a00);
        var p2 = Vector3.Dot(v2, a00);
        var r = boxExtents.Y * Math.Abs(f0.Z) + boxExtents.Z * Math.Abs(f0.Z);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a01
        var a01 = new Vector3(0, -f1.Z, f1.Y);
        p0 = Vector3.Dot(v0, a01);
        p1 = Vector3.Dot(v1, a01);
        p2 = Vector3.Dot(v2, a01);
        r = boxExtents.Y * Math.Abs(f1.Z) + boxExtents.Z * Math.Abs(f1.Y);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a02
        var a02 = new Vector3(0, -f2.Z, f2.Y);
        p0 = Vector3.Dot(v0, a02);
        p1 = Vector3.Dot(v1, a02);
        p2 = Vector3.Dot(v2, a02);
        r = boxExtents.Y * Math.Abs(f2.Z) + boxExtents.Z * Math.Abs(f2.Y);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a10
        var a10 = new Vector3(f0.Z, 0, -f0.X);
        p0 = Vector3.Dot(v0, a10);
        p1 = Vector3.Dot(v1, a10);
        p2 = Vector3.Dot(v2, a10);
        r = boxExtents.X * Math.Abs(f0.Z) + boxExtents.Z * Math.Abs(f0.X);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a11
        var a11 = new Vector3(f1.Z, 0, -f1.X);
        p0 = Vector3.Dot(v0, a11);
        p1 = Vector3.Dot(v1, a11);
        p2 = Vector3.Dot(v2, a11);
        r = boxExtents.X * Math.Abs(f1.Z) + boxExtents.Z * Math.Abs(f1.X);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a12
        var a12 = new Vector3(f2.Z, 0, -f2.X);
        p0 = Vector3.Dot(v0, a12);
        p1 = Vector3.Dot(v1, a12);
        p2 = Vector3.Dot(v2, a12);
        r = boxExtents.X * Math.Abs(f2.Z) + boxExtents.Z * Math.Abs(f2.X);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a20
        var a20 = new Vector3(-f0.Y, f0.X, 0);
        p0 = Vector3.Dot(v0, a20);
        p1 = Vector3.Dot(v1, a20);
        p2 = Vector3.Dot(v2, a20);
        r = boxExtents.X * Math.Abs(f0.Y) + boxExtents.Y * Math.Abs(f0.X);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a21
        var a21 = new Vector3(-f1.Y, f1.X, 0);
        p0 = Vector3.Dot(v0, a21);
        p1 = Vector3.Dot(v1, a21);
        p2 = Vector3.Dot(v2, a21);
        r = boxExtents.X * Math.Abs(f1.Y) + boxExtents.Y * Math.Abs(f1.X);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        // Test axis a22
        var a22 = new Vector3(-f2.Y, f2.X, 0);
        p0 = Vector3.Dot(v0, a22);
        p1 = Vector3.Dot(v1, a22);
        p2 = Vector3.Dot(v2, a22);
        r = boxExtents.X * Math.Abs(f2.Y) + boxExtents.Y * Math.Abs(f2.X);
        if (Math.Max(-fmax(p0, p1, p2), fmin(p0, p1, p2)) > r)
        {
            return false;
        }

        #endregion

        #region Test the three axes corresponding to the face normals of AABB b (category 1)

        // Exit if...
        // ... [-extents.x, extents.x] and [min(v0.x,v1.x,v2.x), max(v0.x,v1.x,v2.x)] do not overlap
        if (fmax(v0.X, v1.X, v2.X) < -boxExtents.X || fmin(v0.X, v1.X, v2.X) > boxExtents.X)
        {
            return false;
        }

        // ... [-extents.y, extents.y] and [min(v0.y,v1.y,v2.y), max(v0.y,v1.y,v2.y)] do not overlap
        if (fmax(v0.Y, v1.Y, v2.Y) < -boxExtents.Y || fmin(v0.Y, v1.Y, v2.Y) > boxExtents.Y)
        {
            return false;
        }

        // ... [-extents.z, extents.z] and [min(v0.z,v1.z,v2.z), max(v0.z,v1.z,v2.z)] do not overlap
        if (fmax(v0.Z, v1.Z, v2.Z) < -boxExtents.Z || fmin(v0.Z, v1.Z, v2.Z) > boxExtents.Z)
        {
            return false;
        }

        #endregion

        #region Test separating axis corresponding to triangle face normal (category 2)

        var planeNormal = Vector3.Cross(f0, f1);
        var planeDistance = Vector3.Dot(planeNormal, v0);

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

    private static float fmin(float a, float b, float c)
    {
        return Math.Min(a, Math.Min(b, c));
    }

    private static float fmax(float a, float b, float c)
    {
        return Math.Max(a, Math.Max(b, c));
    }

    public readonly record struct RayAxisAlignBoxOptimizedIntersection(Vector3 Start, Vector3 InverseDirection)
    {
        public RayAxisAlignBoxOptimizedIntersection(Ray ray) : this(ray.Start, Vector3.One / ray.Direction) { }

        public RayAxisAlignBoxOptimizedIntersection(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection) :
            this(new Vector3(rayTriangleOptimizedIntersection.Start.X,
                             rayTriangleOptimizedIntersection.Start.Y,
                             rayTriangleOptimizedIntersection.Start.Z),
                 Vector3.One / new Vector3(rayTriangleOptimizedIntersection.Direction.X,
                                           rayTriangleOptimizedIntersection.Direction.Y,
                                           rayTriangleOptimizedIntersection.Direction.Z))
        { }
    }
}