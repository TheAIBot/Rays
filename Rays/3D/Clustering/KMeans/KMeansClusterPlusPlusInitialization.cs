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
        bool[] availableItemIndexes = new bool[items.Count];
        Array.Fill(availableItemIndexes, true);
        int[] availableIndexes = new int[items.Count];

        // Choose one center uniformly at random from among the data points.
        int firstIndex = _random.Next(items.Count);
        clusters.Add(new KMeansCluster<T>(items, items.Positions[firstIndex]));
        availableItemIndexes[firstIndex] = false;

        int sampleSize = Math.Min(items.Count, clusterCount * 3);  // Set sample size as needed
        int[] samples = new int[sampleSize];

        float[] newClusterItemDistances = new float[items.Count];
        (float Distance, int Index)[] bestClusterItemDistanceIndexes = new (float, int)[items.Count];
        Array.Fill(bestClusterItemDistanceIndexes, (float.MaxValue, -1));

        for (int i = 1; i < clusterCount; i++)
        {
            newClusterItemDistances = CalculateClusterItemDistances(newClusterItemDistances, clusters[i - 1], items);
            UpdateBestClusterItemDistanceIndexes(newClusterItemDistances, i - 1, bestClusterItemDistanceIndexes);

            samples = GetUniqueRandomValues(samples, availableIndexes.AsSpan(0, availableIndexes.Length - i), availableItemIndexes);

            float totalDistance = 0;
            for (int sampleIndex = 0; sampleIndex < samples.Length; sampleIndex++)
            {
                totalDistance += bestClusterItemDistanceIndexes[sampleIndex].Distance;
            }

            float targetDistance = _random.NextSingle() * totalDistance;
            float cumulativeDistance = 0;
            int chosenSampleIndex = 0;

            for (int sampleIndex = 0; sampleIndex < samples.Length; sampleIndex++)
            {
                cumulativeDistance += bestClusterItemDistanceIndexes[samples[sampleIndex]].Distance;
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
            clusters.Add(new KMeansCluster<T>(items, items.Positions[samples[chosenSampleIndex]]));
            availableItemIndexes[samples[chosenSampleIndex]] = false;
        }

        timer.Stop();
        Console.WriteLine($"Initialization time: {timer.ElapsedMilliseconds:N0}");
        return clusters.ToArray();
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
            if (bestClusterIndex.Item2 == -1 || clusterItemDistances[i] < bestClusterIndex.Item1)
            {
                bestClusterItemDistanceIndexes[i] = (clusterItemDistances[i], newClusterIndex);
            }
        }
    }

    private static int[] GetUniqueRandomValues(int[] samples, Span<int> availableIndexes, bool[] availableItemIndexes)
    {
        int addedValues = 0;
        for (int i = 0; i < availableItemIndexes.Length; i++)
        {
            if (availableItemIndexes[i])
            {
                availableIndexes[addedValues++] = i;
            }
        }

        // Pick random elements from availableIndexes to fill samples
        for (int i = 0; i < samples.Length; i++)
        {
            int randomIndex = _random.Next(availableIndexes.Length);
            samples[i] = availableIndexes[randomIndex];
            availableIndexes[randomIndex] = availableIndexes[availableIndexes.Length - 1]; // Replace with last element
            availableIndexes = availableIndexes.Slice(0, availableIndexes.Length - 1);
        }

        return samples;
    }

    private static int[] GetUniquesssRandomValues(int[] samples, HashSet<int> itemIndexesUsed, int lowerBound, int upperBound)
    {
        HashSet<int> usedIndexes = new HashSet<int>();
        for (int i = 0; i < samples.Length; i++)
        {
            int index;
            do
            {
                index = _random.Next(lowerBound, upperBound);
            } while (usedIndexes.Contains(index) || itemIndexesUsed.Contains(index));
            usedIndexes.Add(index);

            samples[i] = index;
        }

        return samples;
    }
}