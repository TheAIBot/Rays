﻿using System.Numerics;
using WorkProgress;
using WorkProgress.WorkReports.KnownSize;

namespace Clustering.KMeans;

public sealed class ScalarKMeansClusterPlusPlusInitialization : IKMeansClusterInitialization
{
    private readonly KMeansClusterPlusPlusInitialization _kMeansClusterPlusPlusInitialization;
    private readonly IWorkReporting _workReporting;
    private static readonly Random _random = new(2);

    public ScalarKMeansClusterPlusPlusInitialization(KMeansClusterPlusPlusInitialization kMeansClusterPlusPlusInitialization, IWorkReporting workReporting)
    {
        _kMeansClusterPlusPlusInitialization = kMeansClusterPlusPlusInitialization;
        _workReporting = workReporting;
    }

    public KMeansClusters<T> InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        const int stepsInScalarKMeansPlusPlus = 3;
        using IKnownSizeWorkReport workReport = _workReporting.CreateKnownSizeWorkReport(stepsInScalarKMeansPlusPlus);

        int dataPointCount = items.Count; // Number of data points
        int overSamplingFactor = clusterCount; // Oversampling factor, can be adjusted

        KMeansClusters<T> clusters = _kMeansClusterPlusPlusInitialization.InitializeClusters(items, Math.Min(items.Count, (int)(overSamplingFactor * Math.Log(dataPointCount))));
        workReport.IncrementProgress();

        // Phase 2: Weighted reduction to k centers
        // Assign weights to each cluster
        float[] weights = new float[clusters.Count];
        for (int dataPointIndex = 0; dataPointIndex < dataPointCount; dataPointIndex++)
        {
            Vector4 itemPosition = items.Positions[dataPointIndex];
            int nearestClusterIndex = 0;
            float bestDistance = float.MaxValue;
            for (int clusterIndex = 0; clusterIndex < clusters.Count; clusterIndex++)
            {
                float distance = Vector4.DistanceSquared(clusters.GetClusterPosition(clusterIndex), itemPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    nearestClusterIndex = clusterIndex;
                }
            }
            weights[nearestClusterIndex] += 1;
        }
        workReport.IncrementProgress();

        // Use the weights to reduce to k clusters
        var finalClusters = new KMeansClusters<T>(items, clusterCount);
        for (int selectedClusterIndex = 0; selectedClusterIndex < clusterCount; selectedClusterIndex++)
        {
            float totalWeight = weights.Sum();
            float targetWeight = _random.NextSingle() * totalWeight;
            float cumulativeWeight = 0;
            int chosenIndex = 0;
            for (int weightIndex = 0; weightIndex < clusters.Count; weightIndex++)
            {
                cumulativeWeight += weights[weightIndex];
                if (cumulativeWeight >= targetWeight)
                {
                    chosenIndex = weightIndex;
                    break;
                }
            }
            finalClusters.SetClusterPosition(selectedClusterIndex, clusters.GetClusterPosition(chosenIndex));
            weights[chosenIndex] = 0; // Avoid choosing the same cluster again
        }
        workReport.IncrementProgress();

        return finalClusters;
    }
}