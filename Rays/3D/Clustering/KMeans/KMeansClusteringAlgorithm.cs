﻿namespace Rays._3D;

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
        for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
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

            updatedClusters[bestClusterIndex].AddItem(itemIndex);
        }

        //Random random = new Random();
        //float minItemsInCluster = (items.Length / clusters.Length) * 0.1f;
        //int updatedClusterCount = 0;
        //KMeansCluster<T>[] biggestClusters = updatedClusters.OrderByDescending(x => x.ValuesInCluster.Count).ToArray();
        //for (int i = 0; i < updatedClusters.Length; i++)
        //{
        //    if (updatedClusters[i].ValuesInCluster.Count < minItemsInCluster)
        //    {
        //        updatedClusters[i] = new KMeansCluster<T>(biggestClusters[updatedClusterCount].ValuesInCluster[random.Next(0, biggestClusters[updatedClusterCount].ValuesInCluster.Count)].Position);
        //        updatedClusterCount++;
        //    }
        //}

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