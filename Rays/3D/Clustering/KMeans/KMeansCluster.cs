using System.Numerics;

namespace Rays._3D;

public sealed class KMeansCluster<T>
{
    private readonly List<KMeansClusterItem<T>> _valuesInCluster = new List<KMeansClusterItem<T>>();
    private readonly Vector4 _position;

    public IReadOnlyList<KMeansClusterItem<T>> ValuesInCluster => _valuesInCluster;
    public Vector4 Position => _position;


    public KMeansCluster(Vector4 position)
    {
        _position = position;
    }

    public void AddItem(KMeansClusterItem<T> item)
    {
        _valuesInCluster.Add(item);
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
            sum += _valuesInCluster[i].Position;
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
