namespace Clustering.KMeans;

public interface IKMeansClusterScore
{
    float ClusterScore<T>(KMeansClusters<T> clusters, int clusterIndex, KMeansClusterItems<T> items, int itemIndex);

    int GetBestCluster<T>(KMeansClusters<T> clusters, KMeansClusterItems<T> items, int itemIndex);
}
