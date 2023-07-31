using System.Diagnostics;
using System.Numerics;

namespace Rays._3D;

public sealed class KMeansClusterPlusPlusInitialization : IKMeansClusterInitialization
{
    private readonly XorShift _random = new XorShift(2);

    public KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        Stopwatch timer = Stopwatch.StartNew();
        List<KMeansCluster<T>> clusters = new List<KMeansCluster<T>>();
        Span<int> availableItemIndexes = Enumerable.Range(0, items.Count).ToArray();

        // Choose one center uniformly at random from among the data points.
        int firstIndex = _random.Next(items.Count);
        clusters.Add(new KMeansCluster<T>(items, items.Positions[firstIndex]));
        RemoveIndex(ref availableItemIndexes, firstIndex);

        float[] newClusterItemDistances = new float[items.Count];
        (float Distance, int Index)[] bestClusterItemDistanceIndexes = new (float, int)[items.Count];
        Array.Fill(bestClusterItemDistanceIndexes, (float.MaxValue, -1));

        for (int i = 1; i < clusterCount; i++)
        {
            newClusterItemDistances = CalculateClusterItemDistances(newClusterItemDistances, clusters[i - 1], items);
            UpdateBestClusterItemDistanceIndexes(newClusterItemDistances, i - 1, bestClusterItemDistanceIndexes);
            Shuffle(availableItemIndexes);

            float totalDistance = 0;
            for (int distanceIndex = 0; distanceIndex < bestClusterItemDistanceIndexes.Length; distanceIndex++)
            {
                totalDistance += bestClusterItemDistanceIndexes[distanceIndex].Distance;
            }

            float targetDistance = _random.NextSingle() * totalDistance;
            float cumulativeDistance = 0;
            int chosenSampleIndex = 0;

            for (int sampleIndex = 0; sampleIndex < availableItemIndexes.Length; sampleIndex++)
            {
                cumulativeDistance += bestClusterItemDistanceIndexes[availableItemIndexes[sampleIndex]].Distance;
                if (cumulativeDistance >= targetDistance)
                {
                    chosenSampleIndex = sampleIndex;
                    break;
                }
            }

            if (i % 100 == 0)
            {
                Console.WriteLine($"Cluster: {i}/{clusterCount - 1}");
            }
            clusters.Add(new KMeansCluster<T>(items, items.Positions[availableItemIndexes[chosenSampleIndex]]));

            RemoveIndex(ref availableItemIndexes, chosenSampleIndex);
        }

        timer.Stop();
        Console.WriteLine($"Initialization time: {timer.ElapsedMilliseconds:N0}");
        return clusters.ToArray();
    }

    private static void RemoveIndex(ref Span<int> availableItemIndexes, int indexToRmove)
    {
        availableItemIndexes[indexToRmove] = availableItemIndexes[availableItemIndexes.Length - 1];
        availableItemIndexes = availableItemIndexes.Slice(0, availableItemIndexes.Length - 1);
    }

    private static float[] CalculateClusterItemDistances<T>(float[] newClusterItemDistances, KMeansCluster<T> cluster, KMeansClusterItems<T> items)
    {
        Vector4 clusterPosition = cluster.Position;
        for (int i = 0; i < newClusterItemDistances.Length; i++)
        {
            newClusterItemDistances[i] = Vector4.DistanceSquared(clusterPosition, items.Positions[i]);
        }

        return newClusterItemDistances;
    }

    private static void UpdateBestClusterItemDistanceIndexes(float[] clusterItemDistances, int newClusterIndex, (float Distance, int Index)[] bestClusterItemDistanceIndexes)
    {
        for (int i = 0; i < bestClusterItemDistanceIndexes.Length; i++)
        {
            (float, int) bestClusterIndex = bestClusterItemDistanceIndexes[i];
            if (clusterItemDistances[i] < bestClusterIndex.Item1)
            {
                bestClusterItemDistanceIndexes[i] = (clusterItemDistances[i], newClusterIndex);
            }
        }
    }

    // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
    private void Shuffle<T>(Span<T> toShuffle)
    {
        for (int i = 0; i < toShuffle.Length; i++)
        {
            int randomIndex = _random.Next(i, toShuffle.Length);

            T temp = toShuffle[i];
            toShuffle[i] = toShuffle[randomIndex];
            toShuffle[randomIndex] = temp;
        }
    }
}