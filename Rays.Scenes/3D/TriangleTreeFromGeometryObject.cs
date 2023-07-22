using Rays._3D;

namespace Rays.Scenes;

public sealed class TriangleTreeFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly TriangleSetsFromGeometryObject _triangleSetsFromObject;
    private readonly TriangleTreeBuilder _treeBuilder;

    public TriangleTreeFromGeometryObject(TriangleSetsFromGeometryObject triangleSetsFromObject, TriangleTreeBuilder treeBuilder)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
        _treeBuilder = treeBuilder;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        return _treeBuilder.Create(_triangleSetsFromObject.Load(zippedGeometryFilePath));
    }
}
