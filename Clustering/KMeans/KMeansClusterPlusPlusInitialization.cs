﻿using System.Numerics;
using WorkProgress;
using WorkProgress.WorkReports.KnownSize;

namespace Clustering.KMeans;

public sealed class KMeansClusterPlusPlusInitialization : IKMeansClusterInitialization
{
    private readonly XorShift _random = new(2);
    private readonly IWorkReporting _workReporting;

    public KMeansClusterPlusPlusInitialization(IWorkReporting workReporting)
    {
        _workReporting = workReporting;
    }

    public KMeansClusters<T> InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        using IKnownSizeWorkReport workReport = _workReporting.CreateKnownSizeWorkReport(clusterCount);

        var clusters = new KMeansClusters<T>(items, clusterCount);
        int clusterAddedCount = 0;
        Span<int> availableItemIndexes = Enumerable.Range(0, items.Count).ToArray();

        // Choose one center uniformly at random from among the data points.
        int firstIndex = _random.Next(items.Count);
        clusters.SetClusterPosition(clusterAddedCount, items.Positions[firstIndex]);
        clusterAddedCount++;
        RemoveIndex(ref availableItemIndexes, firstIndex);
        workReport.IncrementProgress();

        float[] bestClusterItemDistances = new float[items.Count];
        Array.Fill(bestClusterItemDistances, float.MaxValue);

        for (int i = 1; i < clusterCount; i++)
        {
            float totalDistance = UpdateBestClusterItemDistancesAndGetDistanceSum(clusters.GetClusterPosition(i - 1), items, bestClusterItemDistances);

            float targetDistance = _random.NextSingle() * totalDistance;
            float cumulativeDistance = 0;
            int chosenAvailableItemIndex = -1;

            // Apparently it is more efficient to shuffle chunks of the array
            // instead of shuffling it one index at a time in the inner loop.
            // No idea why that is the case but that is why the shuffle is
            // split into chunks.
            const int shuffleChunkSize = 256;
            int shuffleChunkCount = (availableItemIndexes.Length + (shuffleChunkSize - 1)) / shuffleChunkSize;
            for (int shuffleChunkIndex = 0; shuffleChunkIndex < shuffleChunkCount; shuffleChunkIndex++)
            {
                int startIndex = shuffleChunkIndex * shuffleChunkSize;
                int endIndex = Math.Min(startIndex + shuffleChunkSize, availableItemIndexes.Length);
                PartialYateShuffle(availableItemIndexes, startIndex, endIndex);

                for (int sampleIndex = startIndex; sampleIndex < endIndex; sampleIndex++)
                {
                    cumulativeDistance += bestClusterItemDistances[availableItemIndexes[sampleIndex]];
                    if (cumulativeDistance >= targetDistance)
                    {
                        chosenAvailableItemIndex = sampleIndex;
                        break;
                    }
                }

                if (chosenAvailableItemIndex != -1)
                {
                    break;
                }
            }

            clusters.SetClusterPosition(clusterAddedCount, items.Positions[availableItemIndexes[chosenAvailableItemIndex]]);
            clusterAddedCount++;

            RemoveIndex(ref availableItemIndexes, chosenAvailableItemIndex);
            workReport.IncrementProgress();
        }

        return clusters;
    }

    private static void RemoveIndex(ref Span<int> availableItemIndexes, int indexToRemove)
    {
        availableItemIndexes[indexToRemove] = availableItemIndexes[^1];
        availableItemIndexes = availableItemIndexes[..^1];
    }

    private static float UpdateBestClusterItemDistancesAndGetDistanceSum<T>(Vector4 clusterPosition, KMeansClusterItems<T> items, float[] bestClusterItemDistances)
    {
        float distanceSum = 0;
        for (int i = 0; i < bestClusterItemDistances.Length; i++)
        {
            float newClusterItemDistance = Vector4.DistanceSquared(clusterPosition, items.Positions[i]);
            float bestClusterItemDistance = bestClusterItemDistances[i];
            if (newClusterItemDistance < bestClusterItemDistances[i])
            {
                bestClusterItemDistance = newClusterItemDistance;
                bestClusterItemDistances[i] = bestClusterItemDistance;
            }

            distanceSum += bestClusterItemDistance;
        }

        return distanceSum;
    }

    // Partial implementation of
    // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
    private void PartialYateShuffle<T>(Span<T> toShuffle, int index, int endIndex)
    {
        for (int i = index; i < endIndex; i++)
        {
            int randomIndex = _random.Next(i, toShuffle.Length);

            (toShuffle[i], toShuffle[randomIndex]) = (toShuffle[randomIndex], toShuffle[i]);
        }
    }
}