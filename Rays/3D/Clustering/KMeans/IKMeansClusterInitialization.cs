namespace Rays._3D;

public interface IKMeansClusterInitialization
{
    KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItem<T>[] items, int clusterCount);
}
