using Rays._3D;
using System.Numerics;

namespace Rays.Tests._3D;

public sealed class TriangleIntersectionTests
{
    [Fact]
    public void GetIntersection_CalculatesIntersectionPoint()
    {
        var start = new Vector4(1, 1, 1, 0);
        var direction = new Vector4(2, 3, 4, 0);
        var ray = new Ray(start, direction);
        var rayDirectionMultiplier = 1.5f;
        var triangleIntersection = new TriangleIntersection(rayDirectionMultiplier, 0.3f, 0.4f);

        Vector4 intersection = triangleIntersection.GetIntersection(ray);

        Assert.Equal(start + rayDirectionMultiplier * direction, intersection);
    }

    [Fact]
    public void Interpolate_InterpolatesCornerValues()
    {
        var cornerAValue = new Vector4(1, 1, 1, 0);
        var cornerBValue = new Vector4(2, 2, 2, 0);
        var cornerCValue = new Vector4(3, 3, 3, 0);
        var firstAxisPercent = 0.3f;
        var secondAxisPercent = 0.4f;
        var thirdAxisPercent = 0.3f;
        Vector4 expectedValue = cornerAValue * firstAxisPercent + cornerBValue * secondAxisPercent + cornerCValue * thirdAxisPercent;
        var triangleIntersection = new TriangleIntersection(1.5f, firstAxisPercent, secondAxisPercent);

        Vector4 interpolatedValue = triangleIntersection.Interpolate(cornerAValue, cornerBValue, cornerCValue);

        Assert.Equal(expectedValue, interpolatedValue);
    }
}
