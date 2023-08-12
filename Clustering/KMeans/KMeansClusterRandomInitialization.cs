namespace Clustering.KMeans;

public sealed class KMeansClusterRandomInitialization : IKMeansClusterInitialization
{
    public KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        Random random = new Random(1);
        KMeansCluster<T>[] clusters = new KMeansCluster<T>[clusterCount];
        HashSet<int> notAllowedIndexes = new HashSet<int>();
        for (int i = 0; i < clusters.Length; i++)
        {
            int index;
            do
            {
                index = random.Next(0, items.Count);
            } while (notAllowedIndexes.Contains(index));
            notAllowedIndexes.Add(index);
            clusters[i] = new KMeansCluster<T>(items, items.Positions[index]);
        }

        return clusters;
    }
}
