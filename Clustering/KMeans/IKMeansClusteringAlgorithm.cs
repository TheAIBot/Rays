namespace Clustering.KMeans;

public interface IKMeansClusteringAlgorithm
{
    KMeansClusters<T> CreateClusters<T>(KMeansClusterItems<T> items, int clusterCount);
}
