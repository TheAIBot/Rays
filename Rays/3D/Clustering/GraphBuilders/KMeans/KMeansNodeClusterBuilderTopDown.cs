using Rays._3D;

namespace Clustering.KMeans;

public sealed class KMeansNodeClusterBuilderTopDown : INodeClusterBuilder
{
    private readonly IKMeansClusteringAlgorithm _clusteringAlgorithm;

    public KMeansNodeClusterBuilderTopDown(IKMeansClusteringAlgorithm clusteringAlgorithm)
    {
        _clusteringAlgorithm = clusteringAlgorithm;
    }

    public Node Create(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        TexturedTriangleIndex[] texturedTriangleIndexes = GetTexturedTriangleIndexes(texturedTriangleSets);
        Stack<TriangleIndexNode> nodesToSplit = new Stack<TriangleIndexNode>();
        var rootTriangleIndexNode = new TriangleIndexNode(texturedTriangleIndexes, new Node(Array.Empty<ISubDividableTriangleSet>(), new List<Node>()));
        nodesToSplit.Push(rootTriangleIndexNode);

        while (nodesToSplit.Count > 0)
        {
            TriangleIndexNode parentNode = nodesToSplit.Pop();
            const int maxGroups = 8;
            const int averageTrianglesPerNode = 50;
            int groupCount = Math.Min(maxGroups, (parentNode.TexturedTriangleSets.Length + (averageTrianglesPerNode - 1)) / averageTrianglesPerNode);

            KMeansClusterItems<TexturedTriangleIndex> nodeTriangleIntems = KMeansClusterItems<TexturedTriangleIndex>.Create(parentNode.TexturedTriangleSets, x => texturedTriangleSets[x.TextureIndex].Triangles[x.TriangleIndex].Center);
            KMeansClusters<TexturedTriangleIndex> clusters = _clusteringAlgorithm.CreateClusters(nodeTriangleIntems, groupCount);

            for (int i = 0; i < clusters.Count; i++)
            {
                var childTriangleIndexes = clusters.GetClusterItems(i).ToArray();
                ISubDividableTriangleSet[] childTriangles = clusters.GetClusterItems(i)
                                                                    .Order()
                                                                    .GroupBy(x => x.TextureIndex)
                                                                    .Select(x => texturedTriangleSets[x.Key].SubCopy(x.Select(y => y.TriangleIndex)))
                                                                    .ToArray();
                var child = new TriangleIndexNode(childTriangleIndexes, new Node(childTriangles, new List<Node>()));
                parentNode.Node.Children.Add(child.Node);

                if (child.TexturedTriangleSets.Length > averageTrianglesPerNode)
                {
                    nodesToSplit.Push(child);
                }
            }
        }

        return rootTriangleIndexNode.Node;
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

    private sealed record TriangleIndexNode(TexturedTriangleIndex[] TexturedTriangleSets, Node Node);
}
