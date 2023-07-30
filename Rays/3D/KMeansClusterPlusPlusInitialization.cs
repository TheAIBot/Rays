using System.Numerics;

namespace Rays._3D;

public sealed class KMeansClusterPlusPlusInitialization : IKMeansClusterInitialization
{
    private static Random _random = new Random(1);

    public KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItem<T>[] items, int clusterCount)
    {
        List<KMeansCluster<T>> clusters = new List<KMeansCluster<T>>();
        List<KMeansClusterItem<T>> dataPoints = items.ToList();

        // Choose one center uniformly at random from among the data points.
        int firstIndex = _random.Next(dataPoints.Count);
        clusters.Add(new KMeansCluster<T>(dataPoints[firstIndex].Position));
        dataPoints.RemoveAt(firstIndex);

        int sampleSize = Math.Min(dataPoints.Count, clusterCount * 3);  // Set sample size as needed

        for (int i = 1; i < clusterCount; i++)
        {
            List<KMeansClusterItem<T>> sample = dataPoints.OrderBy(x => _random.Next()).Take(sampleSize).ToList();
            float[] distances = sample.Select(p => clusters.Min(c => Vector4.DistanceSquared(c.Position, p.Position))).ToArray();
            float totalDistance = distances.Sum();

            float targetDistance = _random.NextSingle() * totalDistance;
            float cumulativeDistance = 0;
            int chosenIndex = 0;

            for (int j = 0; j < sample.Count; j++)
            {
                cumulativeDistance += distances[j];
                if (cumulativeDistance >= targetDistance)
                {
                    chosenIndex = j;
                    break;
                }
            }

            clusters.Add(new KMeansCluster<T>(sample[chosenIndex].Position));
            dataPoints.Remove(sample[chosenIndex]);
        }

        return clusters.ToArray();
    }
}
