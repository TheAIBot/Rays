using System.Numerics;

namespace Rays._3D;

public sealed class Statistics<T> where T : struct, INumber<T>
{
    private T _min;
    private T _sum;
    private T _count;
    private T _max;

    public T Min => _min;
    public T Average => _count == default ? default : _sum / _count;
    public T Max => _max;

    public void Update(T value)
    {
        _min = _min == default ? value : T.Min(_min, value);
        _sum += value;
        _count++;
        _max = T.Max(_max, value);
    }

    public void Clear()
    {
        _min = default;
        _sum = default;
        _count = default;
        _max = default;
    }
}