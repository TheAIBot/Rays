using Clustering.KMeans;
using Rays._3D;

namespace Rays.Scenes;

public sealed class KMeansTriangleTreeFromGeometryObject : ITriangleSetIntersectorFromGeometryObject
{
    private readonly TriangleSetsFromGeometryObject _triangleSetsFromObject;
    private readonly TriangleTreeBuilder _treeBuilder;
    private readonly KMeansNodeClusterBuilder _kMeansNodeClusterBuilder;

    public KMeansTriangleTreeFromGeometryObject(TriangleSetsFromGeometryObject triangleSetsFromObject, TriangleTreeBuilder treeBuilder, KMeansNodeClusterBuilder kMeansNodeClusterBuilder)
    {
        _triangleSetsFromObject = triangleSetsFromObject;
        _treeBuilder = treeBuilder;
        _kMeansNodeClusterBuilder = kMeansNodeClusterBuilder;
    }

    public ITriangleSetIntersector Create(string zippedGeometryFilePath)
    {
        var texturedTriangles = _triangleSetsFromObject.Load(zippedGeometryFilePath);
        var rootNode = _kMeansNodeClusterBuilder.Create(texturedTriangles);
        return _treeBuilder.Create(rootNode);
    }
}