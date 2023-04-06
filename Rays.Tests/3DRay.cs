using Rays.Polygons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Rays.Tests;

public sealed class TriangldfeTests
{
    [Fact]
    public void TryGetIntersection_RayIntersectsTriangle_ReturnsTrueAndIntersection()
    {
        // Arrange
        var triangle = new _3D.Triangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        );

        var ray = new _3D.Ray(
            new Vector3(0.5f, 0.5f, 1),
            new Vector3(0, 0, -1)
        );

        Vector3 expectedIntersection = new Vector3(0.5f, 0.5f, 0);

        // Act
        bool intersects = triangle.TryGetIntersection(ray, out Vector3 intersection);

        // Assert
        Assert.True(intersects);
        Assert.Equal(expectedIntersection, intersection);
    }

    [Fact]
    public void TryGetIntersection_RayDoesNotIntersectTriangle_ReturnsFalse()
    {
        // Arrange
        var triangle = new _3D.Triangle(
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0)
        );

        var ray = new _3D.Ray(
            new Vector3(0.5f, 0.5f, 1),
            new Vector3(1, 0, 0)
        );

        // Act
        bool intersects = triangle.TryGetIntersection(ray, out Vector3 intersection);

        // Assert
        Assert.False(intersects);
    }
}
