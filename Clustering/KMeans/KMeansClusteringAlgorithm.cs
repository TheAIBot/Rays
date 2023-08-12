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
            Console.WriteLine($"Updated clusters {awdija.Skip(0).First()}, {awdija.Skip(updatedClusters.Count / 2).First()}, {awdija.Skip(awdija.Length - 1).First()}");
        } while (!clusters.AreSame(oldClusters));

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
