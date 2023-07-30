using System.Numerics;

namespace Rays._3D;

public sealed class KMeansClusterEuclidianScore : IKMeansClusterScore
{
    public float ClusterScore<T>(KMeansCluster<T> cluster, KMeansClusterItem<T> item)
    {
        return Vector4.Distance(cluster.Position, item.Position);
    }
}
