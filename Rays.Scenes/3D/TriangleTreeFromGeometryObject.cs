using Rays._3D;

namespace Rays.Scenes;

public sealed class TriangleTreeFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly TriangleSetsFromGeometryObject _triangleSetsFromObject;

    public TriangleTreeFromGeometryObject(TriangleSetsFromGeometryObject triangleSetsFromObject)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        return TriangleTreeBuilder.Create(_triangleSetsFromObject.Load(zippedGeometryFilePath));
    }
}
