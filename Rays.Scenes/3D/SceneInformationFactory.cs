using Rays._3D;

namespace Rays.Scenes;

public sealed class SceneInformationFactory
{
    public SceneInformation Create(ITriangleSetIntersector triangleSetIntersector)
    {
        Counter triangleCounter = new Counter();
        IEnumerable<Triangle> triangles = TransparentCountTriangles(triangleSetIntersector.GetTriangles(), triangleCounter);
        AxisAlignedBox boundingBox = AxisAlignedBox.GetBoundingBoxForTriangles(triangles);

        return new SceneInformation(triangleCounter.Count, boundingBox);
    }

    private static IEnumerable<Triangle> TransparentCountTriangles(IEnumerable<Triangle> triangles, Counter counter)
    {
        foreach (var triangle in triangles)
        {
            counter.Count++;
            yield return triangle;
        }
    }

    private sealed class Counter
    {
        public int Count = 0;
    }
}
