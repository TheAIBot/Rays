using Rays._3D;
using System.Numerics;

namespace Rays.Tests._3D;

public sealed class AxisAlignedBoxTests
{
    [Theory]
    [InlineData(1, 1, -1, 0, 0, 1)]
    [InlineData(1, 1, 3, 0, 0, -1)]
    [InlineData(-1, 1, 1, 1, 0, 0)]
    [InlineData(3, 1, 1, -1, 0, 0)]
    [InlineData(1, -1, 1, 0, 1, 0)]
    [InlineData(1, 3, 1, 0, -1, 0)]
    [InlineData(1, 1, -1, -0.5f, -0.5f, 1)]
    [InlineData(1, 0.5f, 3, -0.5f, 0, -1)]
    [InlineData(3, 3, 3, -1, -1, -1)]
    [InlineData(1, 1, -3, 0, 0, 1)]
    [InlineData(1, 1, 4, 0, 0, -1)]
    [InlineData(-3, 1, 1, 1, 0, 0)]
    [InlineData(1, 4, 1, 0, -1, 0)]
    public void Intersects_ReturnsTrueForIntersectingRays(
        float rayStartX, float rayStartY, float rayStartZ,
        float rayDirectionX, float rayDirectionY, float rayDirectionZ)
    {
        var minPosition = new Vector3(0, 0, 0);
        var maxPosition = new Vector3(2, 2, 2);
        var box = new AxisAlignedBox(minPosition, maxPosition);

        var rayStart = new Vector3(rayStartX, rayStartY, rayStartZ);
        var rayDirection = new Vector3(rayDirectionX, rayDirectionY, rayDirectionZ);
        var ray = new Ray(rayStart, rayDirection);

        bool intersects = box.Intersects(ray);

        Assert.True(intersects);
    }

    [Theory]
    [InlineData(0, 0, 3, 0.5f, 0, 1)]
    [InlineData(0, 0, 3, 1, 0, 1)]
    [InlineData(3, 3, 0, 1, 1, -1)]
    [InlineData(0, 0, 5, 0, 0, -1)]
    public void Intersects_ReturnsFalseForNonIntersectingRays(
        float rayStartX, float rayStartY, float rayStartZ,
        float rayDirectionX, float rayDirectionY, float rayDirectionZ)
    {
        var minPosition = new Vector3(0, 0, 0);
        var maxPosition = new Vector3(2, 2, 2);
        var box = new AxisAlignedBox(minPosition, maxPosition);

        var rayStart = new Vector3(rayStartX, rayStartY, rayStartZ);
        var rayDirection = new Vector3(rayDirectionX, rayDirectionY, rayDirectionZ);
        var ray = new Ray(rayStart, rayDirection);

        bool intersects = box.Intersects(ray);

        Assert.False(intersects);
    }
}