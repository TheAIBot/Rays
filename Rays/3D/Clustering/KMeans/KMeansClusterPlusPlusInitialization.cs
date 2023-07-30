using System.Diagnostics;
using System.Numerics;

namespace Rays._3D;

public sealed class KMeansClusterPlusPlusInitialization : IKMeansClusterInitialization
{
    private static Random _random = new Random(2);

    public KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        Stopwatch timer = Stopwatch.StartNew();
        List<KMeansCluster<T>> clusters = new List<KMeansCluster<T>>();
        HashSet<int> itemIndexesUsed = new HashSet<int>();

        // Choose one center uniformly at random from among the data points.
        int firstIndex = _random.Next(items.Count);
        clusters.Add(new KMeansCluster<T>(items, items.Positions[firstIndex]));
        itemIndexesUsed.Add(firstIndex);

        int sampleSize = Math.Min(items.Count, clusterCount * 3);  // Set sample size as needed

        float[] distances = new float[sampleSize];
        for (int i = 1; i < clusterCount; i++)
        {
            List<int> sample = GetUniqueRandomValues(itemIndexesUsed, sampleSize, 0, items.Count).ToList();

            Parallel.ForEach(Enumerable.Range(0, sampleSize).Chunk(1), sampleIndexes =>
            {
                foreach (var sampleIndex in sampleIndexes)
                {
                    Vector4 itemPosition = items.Positions[sample[sampleIndex]];

                    float bestDistance = float.MaxValue;
                    for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
                    {
                        bestDistance = Math.Min(bestDistance, Vector4.DistanceSquared(clusters[clusterIndex].Position, itemPosition));
                    }

                    distances[sampleIndex] = bestDistance;
                }
            });

            float totalDistance = distances.Sum();
            float targetDistance = _random.NextSingle() * totalDistance;
            float cumulativeDistance = 0;
            int chosenSampleIndex = 0;

            for (int sampleIndex = 0; sampleIndex < sample.Count; sampleIndex++)
            {
                cumulativeDistance += distances[sampleIndex];
                if (cumulativeDistance >= targetDistance)
                {
                    chosenSampleIndex = sampleIndex;
                    break;
                }
            }

            //Console.WriteLine($"Cluster: {i}/{clusterCount - 1}");
            clusters.Add(new KMeansCluster<T>(items, items.Positions[sample[chosenSampleIndex]]));
            itemIndexesUsed.Add(sample[chosenSampleIndex]);
        }

        timer.Stop();
        Console.WriteLine($"Initialization time: {timer.ElapsedMilliseconds:N0}");
        return clusters.ToArray();
    }

    private static IEnumerable<int> GetUniqueRandomValues(HashSet<int> itemIndexesUsed, int count, int lowerBound, int upperBound)
    {
        HashSet<int> usedIndexes = new HashSet<int>();
        for (int i = 0; i < count; i++)
        {
            int index;
            do
            {
                index = _random.Next(lowerBound, upperBound);
            } while (usedIndexes.Contains(index) || itemIndexesUsed.Contains(index));
            usedIndexes.Add(index);

            yield return index;
        }
    }
}