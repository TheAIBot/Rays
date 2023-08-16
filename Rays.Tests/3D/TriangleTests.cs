using Rays._3D;
using System.Numerics;

namespace Rays.Tests._3D;

public sealed class TriangleTests
{
    [Fact]
    public void TryGetIntersection_IntersectsTriangle_ReturnsTrueAndIntersection()
    {
        var cornerA = new Vector3(0, 0, 0);
        var cornerB = new Vector3(2, 0, 0);
        var cornerC = new Vector3(0, 2, 0);
        var triangle = new Triangle(cornerA, cornerB, cornerC);

        var rayStart = new Vector4(1, 1, 2, 0);
        var rayDirection = new Vector4(0, 0, -1, 0);
        var ray = new Ray(rayStart, rayDirection);

        bool intersects = triangle.TryGetIntersection(ray, out TriangleIntersection intersection);

        Assert.True(intersects);
        Assert.Equal(2.0, intersection.RayDirectionMultiplier, 6);
        Assert.Equal(0.5, intersection.FirstAxisPercent, 6);
        Assert.Equal(0.5, intersection.SecondAxisPercent, 6);
    }

    [Fact]
    public void TryGetIntersection_DoesNotIntersectTriangle_ReturnsFalse()
    {
        var cornerA = new Vector3(0, 0, 0);
        var cornerB = new Vector3(2, 0, 0);
        var cornerC = new Vector3(0, 2, 0);
        var triangle = new Triangle(cornerA, cornerB, cornerC);

        var rayStart = new Vector4(1, 1, 2, 0);
        var rayDirection = new Vector4(1, 0, 0, 0);
        var ray = new Ray(rayStart, rayDirection);
        bool intersects = triangle.TryGetIntersection(ray, out _);

        Assert.False(intersects);
    }
}