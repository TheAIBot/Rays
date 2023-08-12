namespace Clustering.KMeans;

public sealed class KMeansClusterRandomInitialization : IKMeansClusterInitialization
{
    public KMeansClusters<T> InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        var random = new Random(1);
        var clusters = new KMeansClusters<T>(items, clusterCount);
        var notAllowedIndexes = new HashSet<int>();
        for (int i = 0; i < clusters.Count; i++)
        {
            int index;
            do
            {
                index = random.Next(0, items.Count);
            } while (notAllowedIndexes.Contains(index));
            notAllowedIndexes.Add(index);
            clusters.SetClusterPosition(i, items.Positions[index]);
        }

        return clusters;
    }
}
