using System.Numerics;

namespace Clustering.KMeans;

public sealed class KMeansClusterEuclidianScore : IKMeansClusterScore
{
    public float ClusterScore<T>(KMeansCluster<T> cluster, KMeansClusterItems<T> items, int itemIndex)
    {
        return Vector4.Distance(cluster.Position, items.Positions[itemIndex]);
    }
}
