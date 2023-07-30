namespace Rays._3D;

public interface IKMeansClusterScore
{
    float ClusterScore<T>(KMeansCluster<T> cluster, KMeansClusterItems<T> items, int itemIndex);
}
