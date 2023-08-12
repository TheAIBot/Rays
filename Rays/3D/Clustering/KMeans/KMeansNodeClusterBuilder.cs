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

        KMeansClusterItems<TexturedTriangleIndex> triangleClusterItems = KMeansClusterItems<TexturedTriangleIndex>.Create(texturedTriangleIndexes, x => texturedTriangleSets[x.TextureIndex].Triangles[x.TriangleIndex].Center.ToZeroExtendedVector4());
        const int averageTrianglesPerNode = 50;
        int clusterCountToReachAverageTrianglePerNode = (texturedTriangleIndexes.Length + (averageTrianglesPerNode - 1)) / averageTrianglesPerNode;
        KMeansCluster<TexturedTriangleIndex>[] clusters = _clusteringAlgorithm.CreateClusters(triangleClusterItems, clusterCountToReachAverageTrianglePerNode);
        Node[] leafNodes = CreateLeafNodes(clusters, texturedTriangleSets);
        Dictionary<Node, Vector4> nodeToPosition = clusters.Zip(leafNodes).ToDictionary(x => x.Second, x => x.First.Position);
        Node[] lowestLevelNodes = leafNodes;
        while (lowestLevelNodes.Length != 1)
        {
            const int averageChildCount = 8;
            int clusterCountToGetAverageChildCount = (lowestLevelNodes.Length + (averageChildCount - 1)) / averageChildCount;
            KMeansClusterItems<Node> clusterItems = KMeansClusterItems<Node>.Create(lowestLevelNodes, x => nodeToPosition[x]);
            KMeansCluster<Node>[] parentClusters = _clusteringAlgorithm.CreateClusters(clusterItems, clusterCountToGetAverageChildCount);
            List<Node> parentNodes = new List<Node>();
            foreach (var parentCluster in parentClusters)
            {
                Node parentNode = new Node(Array.Empty<ISubDividableTriangleSet>(), parentCluster.Items.ToList());
                nodeToPosition.Add(parentNode, parentCluster.Position);

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

    private static Node[] CreateLeafNodes(KMeansCluster<TexturedTriangleIndex>[] clusters, ISubDividableTriangleSet[] texturedTriangleSets)
    {
        var nodes = new Node[clusters.Length];
        for (int i = 0; i < clusters.Length; i++)
        {
            var nodeTexturedTriangleSets = clusters[i].Items.GroupBy(x => x.TextureIndex)
                                                            .Select(x => texturedTriangleSets[x.Key].SubCopy(x.Select(y => y.TriangleIndex)))
                                                            .ToArray();
            nodes[i] = new Node(nodeTexturedTriangleSets, new List<Node>());
        }

        return nodes;
    }

    private readonly record struct TexturedTriangleIndex(int TextureIndex, int TriangleIndex);
}
