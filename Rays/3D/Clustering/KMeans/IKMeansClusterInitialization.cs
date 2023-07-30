namespace Rays._3D;

public interface IKMeansClusterInitialization
{
    KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount);
}
