namespace Clustering.KMeans;

public interface IKMeansClusteringAlgorithm
{
    KMeansCluster<T>[] CreateClusters<T>(KMeansClusterItems<T> items, int clusterCount);
}
