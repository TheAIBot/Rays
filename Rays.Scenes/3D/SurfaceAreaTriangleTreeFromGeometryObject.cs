using Rays._3D;

namespace Rays.Scenes;

public sealed class SurfaceAreaTriangleTreeFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly ITriangleSetsFromGeometryObject _triangleSetsFromObject;
    private readonly TriangleTreeBuilder _treeBuilder;
    private readonly SurfaceAreaHeuristicNodeClusterBuilder _nodeClusterBuilder;

    public SurfaceAreaTriangleTreeFromGeometryObject(ITriangleSetsFromGeometryObject triangleSetsFromObject, TriangleTreeBuilder treeBuilder, SurfaceAreaHeuristicNodeClusterBuilder nodeClusterBuilder)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
        _treeBuilder = treeBuilder;
        _nodeClusterBuilder = nodeClusterBuilder;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        var texturedTriangles = _triangleSetsFromObject.Load(zippedGeometryFilePath);
        var rootNode = _nodeClusterBuilder.Create(texturedTriangles);
        return _treeBuilder.Create(rootNode);
    }
}