using Rays._3D;

namespace Rays.Scenes;

public sealed class TriangleTreeFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly ITriangleSetsFromGeometryObject _triangleSetsFromObject;
    private readonly TriangleTreeBuilder _treeBuilder;
    private readonly CustomNodeClusterBuilder _customNodeClusterBuilder;

    public TriangleTreeFromGeometryObject(ITriangleSetsFromGeometryObject triangleSetsFromObject, TriangleTreeBuilder treeBuilder, CustomNodeClusterBuilder customNodeClusterBuilder)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
        _treeBuilder = treeBuilder;
        _customNodeClusterBuilder = customNodeClusterBuilder;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        var texturedTriangles = _triangleSetsFromObject.Load(zippedGeometryFilePath);
        var rootNode = _customNodeClusterBuilder.Create(texturedTriangles);
        return _treeBuilder.Create(rootNode);
    }
}