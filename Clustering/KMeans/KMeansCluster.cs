using System.Numerics;

namespace Clustering.KMeans;

public sealed class KMeansCluster<T>
{
    private readonly List<int> _valuesInCluster = new List<int>();
    private readonly KMeansClusterItems<T> _items;
    private readonly Vector4 _position;

    public Vector4 Position => _position;
    public IEnumerable<Vector4> Positions => _valuesInCluster.Select(x => _items.Positions[x]);
    public IEnumerable<T> Items => _valuesInCluster.Select(x => _items.Items[x]);
    public int ItemCount => _valuesInCluster.Count;


    public KMeansCluster(KMeansClusterItems<T> items, Vector4 position)
    {
        _items = items;
        _position = position;
    }

    public void AddItem(int itemIndex)
    {
        _valuesInCluster.Add(itemIndex);
    }

    public Vector4 CalculatePosition()
    {
        if (_valuesInCluster.Count == 0)
        {
            return Position;
        }

        Vector4 sum = Vector4.Zero;
        for (int i = 0; i < _valuesInCluster.Count; i++)
        {
            sum += _items.Positions[_valuesInCluster[i]];
        }

        Vector4 medianPosition = sum / _valuesInCluster.Count;
        return medianPosition;
        //float bestDistance = float.MaxValue;
        //int bestIndex = -1;
        //for (int i = 0; i < _valuesInCluster.Count; i++)
        //{
        //    float distance = Vector4.Distance(medianPosition, _valuesInCluster[i].Position);
        //    if (distance >= bestDistance)
        //    {
        //        continue;
        //    }

        //    bestDistance = distance;
        //    bestIndex = i;
        //}

        //return _valuesInCluster[bestIndex].Position;
    }
}
