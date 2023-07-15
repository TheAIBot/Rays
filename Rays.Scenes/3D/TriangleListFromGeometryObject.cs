using Rays._3D;

namespace Rays.Scenes;

public sealed class TriangleListFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly TriangleSetsFromGeometryObject _triangleSetsFromObject;

    public TriangleListFromGeometryObject(TriangleSetsFromGeometryObject triangleSetsFromObject)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        return new TriangleList(_triangleSetsFromObject.Load(zippedGeometryFilePath));
    }
}
