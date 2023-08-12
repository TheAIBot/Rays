namespace Clustering.KMeans;

public interface IKMeansClusterInitialization
{
    KMeansClusters<T> InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount);
}
