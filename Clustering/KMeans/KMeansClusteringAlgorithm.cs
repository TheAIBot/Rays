using System.Threading.Tasks.Dataflow;

namespace Clustering.KMeans;

public sealed class KMeansClusteringAlgorithm
{
    private readonly IKMeansClusterInitialization _initialization;
    private readonly IKMeansClusterScore _calculateScore;

    public KMeansClusteringAlgorithm(IKMeansClusterInitialization initialization, IKMeansClusterScore calculateScore)
    {
        _initialization = initialization;
        _calculateScore = calculateScore;
    }

    public KMeansCluster<T>[] CreateClusters<T>(KMeansClusterItems<T> items, int clusterCount)
    {
        KMeansCluster<T>[] clusters;
        KMeansCluster<T>[] updatedClusters = _initialization.InitializeClusters(items, clusterCount);
        do
        {
            clusters = updatedClusters;
            updatedClusters = UpdateClusters(clusters, items);

            int[] awdija = updatedClusters.Select(x => x.ItemCount).Order().ToArray();
            Console.WriteLine($"Updated clusters {awdija.Skip(0).First()}, {awdija.Skip(updatedClusters.Length / 2).First()}, {awdija.Skip(awdija.Length - 1).First()}");
        } while (HasClustersChanged(clusters, updatedClusters));

        Console.WriteLine("Finished clusters");
        return updatedClusters;
    }

    private KMeansCluster<T>[] UpdateClusters<T>(KMeansCluster<T>[] clusters, KMeansClusterItems<T> items)
    {
        KMeansCluster<T>[] updatedClusters = clusters.Select(x => new KMeansCluster<T>(items, x.CalculatePosition())).ToArray();
        var transformer = new TransformBlock<int, (int ClusterIndex, int ItemIndex)>(itemIndex =>
        {
            int bestClusterIndex = -1;
            float bestClusterScore = float.MaxValue;
            for (int i = 0; i < updatedClusters.Length; i++)
            {
                float score = _calculateScore.ClusterScore(updatedClusters[i], items, itemIndex);
                if (score >= bestClusterScore)
                {
                    continue;
                }

                bestClusterScore = score;
                bestClusterIndex = i;
            }

            return (bestClusterIndex, itemIndex);
        }, new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            SingleProducerConstrained = true
        });

        for (int i = 0; i < items.Count; i++)
        {
            transformer.Post(i);
        }
        transformer.Complete();

        foreach ((int clusterIndex, int itemIndex) in transformer.ReceiveAllAsync().ToBlockingEnumerable())
        {
            updatedClusters[clusterIndex].AddItem(itemIndex);
        }

        //Random random = new Random();
        //float minItemsInCluster = (items.Count / clusters.Length) * 0.1f;

        return updatedClusters;
    }

    private static bool HasClustersChanged<T>(KMeansCluster<T>[] oldClusters, KMeansCluster<T>[] updatedClusters)
    {
        for (int i = 0; i < oldClusters.Length; i++)
        {
            if (!oldClusters[i].Items.SequenceEqual(updatedClusters[i].Items))
            {
                return true;
            }
        }

        return false;
    }
}
