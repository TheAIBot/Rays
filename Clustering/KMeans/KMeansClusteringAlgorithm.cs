using WorkProgress;
using WorkProgress.WorkReports.UnknownSize;

namespace Clustering.KMeans;

public sealed class KMeansClusteringAlgorithm : IKMeansClusteringAlgorithm
{
    private readonly IKMeansClusterInitialization _initialization;
    private readonly IKMeansClusterScore _calculateScore;
    private readonly IWorkReporting _workReporting;

    public KMeansClusteringAlgorithm(IKMeansClusterInitialization initialization, IKMeansClusterScore calculateScore, IWorkReporting workReporting)
    {
        _initialization = initialization;
        _calculateScore = calculateScore;
        _workReporting = workReporting;
    }

    public KMeansClusters<T> CreateClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        using IUnknownSizeWorkReport workReport = _workReporting.CreateUnknownWorkReport();

        KMeansClusters<T> clusters = _initialization.InitializeClusters(items, clusterCount);
        KMeansClusters<T> oldClusters = new KMeansClusters<T>(items, clusterCount);
        do
        {
            KMeansClusters<T> updatedClusters = UpdateClusters(clusters, oldClusters, items);

            oldClusters = clusters;
            clusters = updatedClusters;

            workReport.IncrementProgress();
            int[] awdija = updatedClusters.GetClusterItemCounts().Order().ToArray();
            int percentile000 = awdija.Skip((updatedClusters.Count / 4) * 0).First();
            int percentile025 = awdija.Skip((updatedClusters.Count / 4) * 1).First();
            int percentile050 = awdija.Skip((updatedClusters.Count / 4) * 2).First();
            int percentile075 = awdija.Skip((updatedClusters.Count / 4) * 3).First();
            int percentile100 = awdija.Skip(updatedClusters.Count - 1).First();
            Console.WriteLine($"Updated clusters {percentile000}, {percentile025}, {percentile050}, {percentile075}, {percentile100}");
        } while (!clusters.Equals(oldClusters));

        workReport.Complete();
        Console.WriteLine($"Finished clusters: {workReport.WorkTime.TotalMilliseconds:N0}");
        return clusters;
    }

    private KMeansClusters<T> UpdateClusters<T>(KMeansClusters<T> clusters, KMeansClusters<T> updatedClusters, KMeansClusterItems<T> items)
    {
        updatedClusters.UpdateClusterPositionsFromClusters(clusters);

        Parallel.For(0, items.Count, itemIndex =>
        {
            int bestClusterIndex = _calculateScore.GetBestCluster(updatedClusters, items, itemIndex);
            updatedClusters.SetClusterItem(bestClusterIndex, itemIndex);
        });

        return updatedClusters;
    }
}
