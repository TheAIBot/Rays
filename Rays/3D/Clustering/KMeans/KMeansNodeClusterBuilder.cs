using System.Numerics;

namespace Rays._3D;

public sealed class KMeansNodeClusterBuilder : INodeClusterBuilder
{
    private readonly KMeansClusteringAlgorithm _clusteringAlgorithm;

    public KMeansNodeClusterBuilder(KMeansClusteringAlgorithm clusteringAlgorithm)
    {
        _clusteringAlgorithm = clusteringAlgorithm;
    }

    public Node Create(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        Triangle[] allTriangles = texturedTriangleSets.SelectMany(x => x.GetTriangles()).ToArray();
        KMeansClusterItems<Triangle> triangleClusterItems = KMeansClusterItems<Triangle>.Create(allTriangles, x => x.Center.ToZeroExtendedVector4());
        const int averageTrianglesPerNode = 50;
        int clusterCountToReachAverageTrianglePerNode = (allTriangles.Length + (averageTrianglesPerNode - 1)) / averageTrianglesPerNode;
        KMeansCluster<Triangle>[] clusters = _clusteringAlgorithm.CreateClusters(triangleClusterItems, clusterCountToReachAverageTrianglePerNode);
        Node[] leafNodes = clusters.Select(x => x.Items.ToHashSet())
                                   .Select(x => new Node(texturedTriangleSets.Select(y => y.SubCopy(z => x.Contains(z)))
                                                                             .Where(y => y.Triangles.Length > 0)
                                                                             .ToArray(), new List<Node>()))
                                   .ToArray();
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
}
