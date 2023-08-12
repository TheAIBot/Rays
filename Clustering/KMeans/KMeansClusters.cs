using System.Numerics;

namespace Clustering.KMeans;

public sealed class KMeansClusters<T>
{
    private readonly KMeansClusterItems<T> _items;
    private readonly int[] _itemCluster;
    private readonly Vector4[] _clusterPositions;
    private const int _noCluster = -1;

    public int Count => _clusterPositions.Length;

    public KMeansClusters(KMeansClusterItems<T> items, int clusterCount)
    {
        _items = items;
        _itemCluster = new int[_items.Count];
        Array.Fill(_itemCluster, _noCluster);
        _clusterPositions = new Vector4[clusterCount];
    }

    public void SetClusterItem(int clusterIndex, int itemIndex)
    {
        _itemCluster[itemIndex] = clusterIndex;
    }

    public IEnumerable<T> GetClusterItems(int clusterIndex)
    {
        for (int i = 0; i < _itemCluster.Length; i++)
        {
            if (_itemCluster[i] == clusterIndex)
            {
                yield return _items.Items[i];
            }
        }
    }

    public void SetClusterPosition(int clusterIndex, Vector4 position)
    {
        _clusterPositions[clusterIndex] = position;
    }

    public ReadOnlySpan<Vector4> GetClusterPositions() => _clusterPositions;

    public Vector4 GetClusterPosition(int clusterIndex) => _clusterPositions[clusterIndex];

    public Vector4 CalculateClusterPosition(int clusterIndex)
    {
        Vector4 sum = Vector4.Zero;
        int itemCount = 0;
        for (int i = 0; i < _itemCluster.Length; i++)
        {
            if (_itemCluster[i] == clusterIndex)
            {
                sum += _items.Positions[i];
                itemCount++;
            }
        }

        if (itemCount == 0)
        {
            return _clusterPositions[clusterIndex];
        }

        Vector4 medianPosition = sum / itemCount;
        return medianPosition;
    }

    public void UpdateClusterPositionsFromClusters(KMeansClusters<T> otherClusters)
    {
        Array.Clear(_clusterPositions);

        int[] clustersItemCount = new int[Count];
        for (int i = 0; i < otherClusters._itemCluster.Length; i++)
        {
            int clusterIndex = otherClusters._itemCluster[i];
            if (clusterIndex == _noCluster)
            {
                continue;
            }

            _clusterPositions[clusterIndex] += _items.Positions[i];
            clustersItemCount[clusterIndex]++;
        }

        for (int i = 0; i < Count; i++)
        {
            if (clustersItemCount[i] == 0)
            {
                _clusterPositions[i] = otherClusters._clusterPositions[i];
                continue;
            }

            _clusterPositions[i] /= clustersItemCount[i];
        }
    }

    public int[] GetClusterItemCounts()
    {
        int[] clustersItemCount = new int[Count];
        for (int i = 0; i < _itemCluster.Length; i++)
        {
            int clusterIndex = _itemCluster[i];
            if (clusterIndex == _noCluster)
            {
                continue;
            }

            clustersItemCount[clusterIndex]++;
        }

        return clustersItemCount;
    }

    public bool AreSame(KMeansClusters<T> other)
    {
        return _itemCluster.SequenceEqual(other._itemCluster);
    }
}