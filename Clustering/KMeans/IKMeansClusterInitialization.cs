namespace Clustering.KMeans;

public interface IKMeansClusterInitialization
{
    KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount);
}
