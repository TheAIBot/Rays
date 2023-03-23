using Rays.Polygons;
using System.Numerics;

namespace Rays.Tests;

public class IntersectionTests
{
    public class IntersectionTestData : TheoryData<Ray, Wall, Vector2?>
    {
        public IntersectionTestData()
        {
            // Case 1: Ray and wall intersect at one point
            // | /
            // |/#
            // | #
            // | #
            // ----
            Add(
                new Ray(new Vector2(0, 0), new Vector2(1, 1)),
                new Wall(new Line(new Vector2(1, 1), new Vector2(1, 3)), new Vector2(1, 1)),
                new Vector2(1, 1)
            );

            // Case 2: Ray doesn't intersect the wall, but would if extended backward
            //| x
            //|  /
            //|  #
            //| #
            // -----
            Add(
                new Ray(new Vector2(2, 2), new Vector2(1, 1)),
                new Wall(new Line(new Vector2(1, 0), new Vector2(1, 1)), new Vector2(1, 1)),
                null
            );

            // Case 3: Ray doesn't intersect the wall
            //| x
            //|  /
            //| /
            //|/
            //-----
            Add(
                new Ray(new Vector2(0, 0), new Vector2(1, 1)),
                new Wall(new Line(new Vector2(2, 2), new Vector2(3, 3)), new Vector2(1, 1)),
                null
            );
        }
    }

    [Theory]
    [ClassData(typeof(IntersectionTestData))]
    public void TestTryGetIntersectionPointAndTryGetIntersectionPointVectorized(Ray ray, Wall wall, Vector2? expectedIntersection)
    {
        Vector2? intersectionScalar = wall.TryGetIntersectionPointScalar(ray);
        Vector2? intersectionVectorized = wall.TryGetIntersectionPointVectorized(ray);

        Assert.Equal(expectedIntersection, intersectionScalar);
        Assert.Equal(expectedIntersection, intersectionVectorized);
    }
}