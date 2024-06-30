using Rays._3D;
using System.Numerics;

namespace Clustering.KMeans;

public sealed class KMeansNodeClusterBuilder : INodeClusterBuilder
{
    private readonly IKMeansClusteringAlgorithm _clusteringAlgorithm;

    public KMeansNodeClusterBuilder(IKMeansClusteringAlgorithm clusteringAlgorithm)
    {
        _clusteringAlgorithm = clusteringAlgorithm;
    }

    public Node Create(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        TexturedTriangleIndex[] texturedTriangleIndexes = GetTexturedTriangleIndexes(texturedTriangleSets);

        KMeansClusterItems<TexturedTriangleIndex> triangleClusterItems = KMeansClusterItems<TexturedTriangleIndex>.Create(texturedTriangleIndexes, x => texturedTriangleSets[x.TextureIndex].Triangles[x.TriangleIndex].Center);
        const int averageTrianglesPerNode = 50;
        int clusterCountToReachAverageTrianglePerNode = (texturedTriangleIndexes.Length + (averageTrianglesPerNode - 1)) / averageTrianglesPerNode;
        KMeansClusters<TexturedTriangleIndex> clusters = _clusteringAlgorithm.CreateClusters(triangleClusterItems, clusterCountToReachAverageTrianglePerNode);
        Node[] leafNodes = CreateLeafNodes(clusters, texturedTriangleSets);
        Dictionary<Node, Vector4> nodeToPosition = Enumerable.Range(0, clusters.Count)
                                                             .Select(x => clusters.GetClusterPosition(x))
                                                             .Zip(leafNodes)
                                                             .ToDictionary(x => x.Second, x => x.First);
        Node[] lowestLevelNodes = leafNodes;
        while (lowestLevelNodes.Length != 1)
        {
            const int averageChildCount = 8;
            int clusterCountToGetAverageChildCount = (lowestLevelNodes.Length + (averageChildCount - 1)) / averageChildCount;
            KMeansClusterItems<Node> clusterItems = KMeansClusterItems<Node>.Create(lowestLevelNodes, x => nodeToPosition[x]);
            KMeansClusters<Node> parentClusters = _clusteringAlgorithm.CreateClusters(clusterItems, clusterCountToGetAverageChildCount);
            var parentNodes = new List<Node>();
            for (int parentClusterIndex = 0; parentClusterIndex < parentClusters.Count; parentClusterIndex++)
            {
                var parentNode = new Node(Array.Empty<ISubDividableTriangleSet>(), parentClusters.GetClusterItems(parentClusterIndex).ToList());
                nodeToPosition.Add(parentNode, parentClusters.GetClusterPosition(parentClusterIndex));

                parentNodes.Add(parentNode);
            }

            lowestLevelNodes = parentNodes.ToArray();
        }

        return lowestLevelNodes.Single();
    }

    private static TexturedTriangleIndex[] GetTexturedTriangleIndexes(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        var texturedTriangleIndexes = new TexturedTriangleIndex[texturedTriangleSets.Sum(x => x.Triangles.Length)];
        var triangleIndex = 0;
        for (int textureIndex = 0; textureIndex < texturedTriangleSets.Length; textureIndex++)
        {
            for (int i = 0; i < texturedTriangleSets[textureIndex].Triangles.Length; i++)
            {
                texturedTriangleIndexes[triangleIndex++] = new TexturedTriangleIndex(textureIndex, i);
            }
        }

        return texturedTriangleIndexes;
    }

    private static Node[] CreateLeafNodes(KMeansClusters<TexturedTriangleIndex> clusters, ISubDividableTriangleSet[] texturedTriangleSets)
    {
        var nodes = new Node[clusters.Count];
        for (int i = 0; i < clusters.Count; i++)
        {
            var nodeTexturedTriangleSets = clusters.GetClusterItems(i)
                                                   .GroupBy(x => x.TextureIndex)
                                                   .Select(x => texturedTriangleSets[x.Key].SubCopy(x.Select(y => y.TriangleIndex)))
                                                   .ToArray();
            nodes[i] = new Node(nodeTexturedTriangleSets, new List<Node>());
        }

        return nodes;
    }

    private readonly record struct TexturedTriangleIndex(int TextureIndex, int TriangleIndex);
}
