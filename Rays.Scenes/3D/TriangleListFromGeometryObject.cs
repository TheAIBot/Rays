using Rays._3D;

namespace Rays.Scenes;

public sealed class TriangleListFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly ITriangleSetsFromGeometryObject _triangleSetsFromObject;

    public TriangleListFromGeometryObject(ITriangleSetsFromGeometryObject triangleSetsFromObject)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        return new TriangleList(_triangleSetsFromObject.Load(zippedGeometryFilePath));
    }
}
