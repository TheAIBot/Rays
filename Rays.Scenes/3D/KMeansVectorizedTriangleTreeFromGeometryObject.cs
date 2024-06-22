using Clustering.KMeans;
using Rays._3D;

namespace Rays.Scenes;

public sealed class KMeansVectorizedTriangleTreeFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly ITriangleSetsFromGeometryObject _triangleSetsFromObject;
    private readonly VectorizedTriangleTreeBuilder _treeBuilder;
    private readonly INodeClusterBuilder _nodeClusterBuilder;

    public KMeansVectorizedTriangleTreeFromGeometryObject(ITriangleSetsFromGeometryObject triangleSetsFromObject,
                                                          VectorizedTriangleTreeBuilder treeBuilder,
                                                          KMeansNodeClusterBuilder kMeansNodeClusterBuilder)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
        _treeBuilder = treeBuilder;
        _nodeClusterBuilder = kMeansNodeClusterBuilder;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        var texturedTriangles = _triangleSetsFromObject.Load(zippedGeometryFilePath);
        var rootNode = _nodeClusterBuilder.Create(texturedTriangles);
        return _treeBuilder.Create(rootNode);
    }
}