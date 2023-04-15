using Rays._3D;
using System.Numerics;

namespace Rays.Tests._3D;

public sealed class TriangleIntersectionTests
{
    [Fact]
    public void GetIntersection_CalculatesIntersectionPoint()
    {
        var start = new Vector3(1, 1, 1);
        var direction = new Vector3(2, 3, 4);
        var ray = new Ray(start, direction);
        var rayDirectionMultiplier = 1.5f;
        var triangleIntersection = new TriangleIntersection(rayDirectionMultiplier, 0.3f, 0.4f);

        Vector3 intersection = triangleIntersection.GetIntersection(ray);

        Assert.Equal(start + rayDirectionMultiplier * direction, intersection);
    }

    [Fact]
    public void Interpolate_InterpolatesCornerValues()
    {
        var cornerAValue = new Vector3(1, 1, 1);
        var cornerBValue = new Vector3(2, 2, 2);
        var cornerCValue = new Vector3(3, 3, 3);
        var firstAxisPercent = 0.3f;
        var secondAxisPercent = 0.4f;
        var thirdAxisPercent = 0.3f;
        Vector3 expectedValue = cornerAValue * firstAxisPercent + cornerBValue * secondAxisPercent + cornerCValue * thirdAxisPercent;
        var triangleIntersection = new TriangleIntersection(1.5f, firstAxisPercent, secondAxisPercent);

        Vector3 interpolatedValue = triangleIntersection.Interpolate(cornerAValue, cornerBValue, cornerCValue);

        Assert.Equal(expectedValue, interpolatedValue);
    }
}
