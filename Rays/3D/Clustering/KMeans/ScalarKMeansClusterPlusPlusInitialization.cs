using System.Numerics;
using WorkProgress;

namespace Rays._3D;

public sealed class ScalarKMeansClusterPlusPlusInitialization : IKMeansClusterInitialization
{
    private readonly KMeansClusterPlusPlusInitialization _kMeansClusterPlusPlusInitialization;
    private readonly IWorkReporting _workReporting;
    private static Random _random = new Random(2);

    public ScalarKMeansClusterPlusPlusInitialization(KMeansClusterPlusPlusInitialization kMeansClusterPlusPlusInitialization, IWorkReporting workReporting)
    {
        _kMeansClusterPlusPlusInitialization = kMeansClusterPlusPlusInitialization;
        _workReporting = workReporting;
    }

    public KMeansCluster<T>[] InitializeClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        const int stepsInScalarKMeansPlusPlus = 3;
        using IKnownSizeWorkReport workReport = _workReporting.CreateKnownSizeWorkReport(stepsInScalarKMeansPlusPlus);

        int dataPointCount = items.Count; // Number of data points
        int overSamplingFactor = clusterCount; // Oversampling factor, can be adjusted

        KMeansCluster<T>[] clusters = _kMeansClusterPlusPlusInitialization.InitializeClusters(items, Math.Min(items.Count, (int)(overSamplingFactor * Math.Log(dataPointCount))));
        workReport.IncrementProgress();

        // Phase 2: Weighted reduction to k centers
        // Assign weights to each cluster
        float[] weights = new float[clusters.Length];
        for (int dataPointIndex = 0; dataPointIndex < dataPointCount; dataPointIndex++)
        {
            Vector4 itemPosition = items.Positions[dataPointIndex];
            int nearestClusterIndex = 0;
            float bestDistance = float.MaxValue;
            for (int clusterIndex = 0; clusterIndex < clusters.Length; clusterIndex++)
            {
                float distance = Vector4.DistanceSquared(clusters[clusterIndex].Position, itemPosition);
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
        List<KMeansCluster<T>> finalClusters = new List<KMeansCluster<T>>();
        for (int selectedClusterIndex = 0; selectedClusterIndex < clusterCount; selectedClusterIndex++)
        {
            float totalWeight = weights.Sum();
            float targetWeight = _random.NextSingle() * totalWeight;
            float cumulativeWeight = 0;
            int chosenIndex = 0;
            for (int weightIndex = 0; weightIndex < clusters.Length; weightIndex++)
            {
                cumulativeWeight += weights[weightIndex];
                if (cumulativeWeight >= targetWeight)
                {
                    chosenIndex = weightIndex;
                    break;
                }
            }
            finalClusters.Add(clusters[chosenIndex]);
            weights[chosenIndex] = 0; // Avoid choosing the same cluster again
        }
        workReport.IncrementProgress();

        return finalClusters.ToArray();
    }
}