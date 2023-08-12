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

        KMeansClusters<T> clusters;
        KMeansClusters<T> updatedClusters = _initialization.InitializeClusters(items, clusterCount);
        do
        {
            clusters = updatedClusters;
            updatedClusters = UpdateClusters(clusters, items);

            workReport.IncrementProgress();
            //int[] awdija = updatedClusters.Select(x => x.ItemCount).Order().ToArray();
            //Console.WriteLine($"Updated clusters {awdija.Skip(0).First()}, {awdija.Skip(updatedClusters.Length / 2).First()}, {awdija.Skip(awdija.Length - 1).First()}");
        } while (!clusters.AreSame(updatedClusters));

        workReport.Complete();
        Console.WriteLine($"Finished clusters: {workReport.WorkTime.TotalMilliseconds:N0}");
        return updatedClusters;
    }

    private KMeansClusters<T> UpdateClusters<T>(KMeansClusters<T> clusters, KMeansClusterItems<T> items)
    {
        KMeansClusters<T> updatedClusters = new KMeansClusters<T>(items, clusters.Count);
        updatedClusters.UpdateClusterPositionsFromClusters(clusters);

        Parallel.For(0, items.Count, itemIndex =>
        {
            int bestClusterIndex = _calculateScore.GetBestCluster(updatedClusters, items, itemIndex);
            updatedClusters.SetClusterItem(bestClusterIndex, itemIndex);
        });

        return updatedClusters;
    }
}
