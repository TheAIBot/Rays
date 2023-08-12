using System.Numerics;

namespace Clustering.KMeans;

public sealed class KMeansClusterEuclidianScore : IKMeansClusterScore
{
    public float ClusterScore<T>(KMeansClusters<T> clusters, int clusterIndex, KMeansClusterItems<T> items, int itemIndex)
    {
        return Vector4.Distance(clusters.GetClusterPosition(clusterIndex), items.Positions[itemIndex]);
    }

    public int GetBestCluster<T>(KMeansClusters<T> clusters, KMeansClusterItems<T> items, int itemIndex)
    {
        Vector4 itemPosition = items.Positions[itemIndex];
        int bestClusterIndex = -1;
        float bestClusterScore = float.MaxValue;
        ReadOnlySpan<Vector4> clusterPositions = clusters.GetClusterPositions();
        for (int i = 0; i < clusterPositions.Length; i++)
        {
            float score = Vector4.Distance(clusterPositions[i], itemPosition);
            if (score >= bestClusterScore)
            {
                continue;
            }

            bestClusterScore = score;
            bestClusterIndex = i;
        }

        return bestClusterIndex;
    }
}
