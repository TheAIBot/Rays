using Rays._3D;
using System.Numerics;
using System.Threading.Tasks.Dataflow;

namespace Clustering.KMeans;

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

    private static Node[] CreateLeafNodes(KMeansCluster<Triangle>[] clusters, ISubDividableTriangleSet[] texturedTriangleSets)
    {
        var leafNodeCreator = new TransformBlock<KMeansCluster<Triangle>, Node>(cluster =>
        {
            var clusterTriangles = cluster.Items.ToHashSet();
            var clusterTexturedTriangles = texturedTriangleSets.Select(x => x.SubCopy(y => clusterTriangles.Contains(y)))
                                                               .Where(x => x.Triangles.Length > 0)
                                                               .ToArray();
            return new Node(clusterTexturedTriangles, new List<Node>());
        }, new ExecutionDataflowBlockOptions()
        {
            EnsureOrdered = true,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            SingleProducerConstrained = true,
        });

        foreach (var cluster in clusters)
        {
            leafNodeCreator.Post(cluster);
        }
        leafNodeCreator.Complete();

        return leafNodeCreator.ReceiveAllAsync()
                              .ToBlockingEnumerable()
                              .ToArray();
    }
}
