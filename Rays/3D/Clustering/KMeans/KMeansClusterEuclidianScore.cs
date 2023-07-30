using System.Numerics;

namespace Rays._3D;

public sealed class KMeansClusterEuclidianScore : IKMeansClusterScore
{
    public float ClusterScore<T>(KMeansCluster<T> cluster, KMeansClusterItems<T> items, int itemIndex)
    {
        return Vector4.Distance(cluster.Position, items.Positions[itemIndex]);
    }
}
