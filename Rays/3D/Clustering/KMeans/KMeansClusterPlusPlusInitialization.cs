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
        int[] samples = new int[sampleSize];

        float[] newClusterItemDistances = new float[items.Count];
        (float Distance, int Index)[] bestClusterItemDistanceIndexes = new (float, int)[items.Count];
        Array.Fill(bestClusterItemDistanceIndexes, (float.MaxValue, -1));

        for (int i = 1; i < clusterCount; i++)
        {
            newClusterItemDistances = CalculateClusterItemDistances(newClusterItemDistances, clusters[i - 1], items);
            UpdateBestClusterItemDistanceIndexes(newClusterItemDistances, i - 1, bestClusterItemDistanceIndexes);

            samples = GetUniqueRandomValues(samples, itemIndexesUsed, 0, items.Count);

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
            itemIndexesUsed.Add(samples[chosenSampleIndex]);
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

    private static int[] GetUniqueRansssdomValues(int[] samples, HashSet<int> itemIndexesUsed, int lowerBound, int upperBound)
    {
        List<int> availableIndexes = new List<int>(upperBound - lowerBound - itemIndexesUsed.Count);

        // Fill the availableIndexes list with values that are not in itemIndexesUsed
        for (int i = lowerBound; i < upperBound; i++)
        {
            if (!itemIndexesUsed.Contains(i))
            {
                availableIndexes.Add(i);
            }
        }

        // Pick random elements from availableIndexes to fill samples
        for (int i = 0; i < samples.Length; i++)
        {
            int randomIndex = _random.Next(availableIndexes.Count);
            samples[i] = availableIndexes[randomIndex];
            availableIndexes[randomIndex] = availableIndexes[availableIndexes.Count - 1]; // Replace with last element
            availableIndexes.RemoveAt(availableIndexes.Count - 1); // Remove last element
        }

        return samples;
    }

    private static int[] GetUniqueRandomValues(int[] samples, HashSet<int> itemIndexesUsed, int lowerBound, int upperBound)
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