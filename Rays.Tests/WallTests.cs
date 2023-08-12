using Rays.Polygons;
using System.Numerics;

namespace Rays.Tests;

public sealed class WallTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 0.1f)]
    [InlineData(1, -0.1f)]
    [InlineData(1, 1)]
    [InlineData(1, -1)]
    public void TryReflectRay_WithRayPointingSomewhatRight_ExpectRayReflected(float dirX, float dirY)
    {
        var ray = new Ray(new Vector2(0, 0), new Vector2(dirX * 10, dirY * 10));
        var wall = new Wall(new Line(new Vector2(1, 3), new Vector2(1, -3)), new Vector2(-1, 0));

        Ray? reflectedRay = wall.TryReflectRay(ray);

        Assert.True(reflectedRay.HasValue);
        Assert.Equal(-dirX * 10, reflectedRay.Value.Direction.X, 0.00001f);
        Assert.Equal(dirY * 10, reflectedRay.Value.Direction.Y, 0.00001f);
        Assert.Equal(dirX, reflectedRay.Value.Start.X, 0.00001f);
        Assert.Equal(dirY, reflectedRay.Value.Start.Y, 0.00001f);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 0.1f)]
    [InlineData(1, -0.1f)]
    [InlineData(1, 1)]
    [InlineData(1, -1)]
    public void TryReflectRay_WithRayPointingSomewhatRightWallTwiceAsFarAway_ExpectRayReflected(float dirX, float dirY)
    {
        var ray = new Ray(new Vector2(0, 0), new Vector2(dirX, dirY));
        var wall = new Wall(new Line(new Vector2(2, 3), new Vector2(2, -3)), new Vector2(-1, 0));

        Ray? reflectedRay = wall.TryReflectRay(ray);

        Assert.True(reflectedRay.HasValue);
        Assert.Equal(-dirX, reflectedRay.Value.Direction.X, 0.00001f);
        Assert.Equal(dirY, reflectedRay.Value.Direction.Y, 0.00001f);
        Assert.Equal(dirX * 2, reflectedRay.Value.Start.X, 0.00001f);
        Assert.Equal(dirY * 2, reflectedRay.Value.Start.Y, 0.00001f);
    }

    public static TheoryData<Ray, Wall, Vector2?> IntersectionTestData => new()
    {
        {
            new Ray(new Vector2(0, 0), new Vector2(1, 1)),
            new Wall(new Line(new Vector2(1, 1), new Vector2(1, 3)), Vector2.Zero),
            new Vector2(1, 1)
        },
        {
            new Ray(new Vector2(2, 2), new Vector2(1, 1)),
            new Wall(new Line(new Vector2(1, 0), new Vector2(1, 1)), Vector2.Zero),
            null
        },
        {
            new Ray(new Vector2(0, 0), new Vector2(1, 1)),
            new Wall(new Line(new Vector2(2, 2), new Vector2(3, 3)), Vector2.Zero),
            null
        }
    };

    [Theory]
    [MemberData(nameof(IntersectionTestData))]
    public void TestTryGetIntersectionPointScalar(Ray ray, Wall wall, Vector2? expectedIntersection)
    {
        Vector2? intersectionScalar = wall.TryGetIntersectionPointScalar(ray);

        Assert.Equal(expectedIntersection, intersectionScalar);
    }
}