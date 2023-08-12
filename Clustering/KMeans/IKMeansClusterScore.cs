namespace Clustering.KMeans;

public interface IKMeansClusterScore
{
    float ClusterScore<T>(KMeansCluster<T> cluster, KMeansClusterItems<T> items, int itemIndex);
}
