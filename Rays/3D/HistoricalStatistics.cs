using System.Numerics;

namespace Rays._3D;

public readonly record struct HistoricalStatistics<T>(int MaxHistoryLength) where T : struct, INumber<T>
{
    private readonly Queue<Statistics<T>> _statistics = new();

    public T Min => CalculateMin();
    public T Average => CalculateAverage();
    public T Max => CalculateMax();

    public void AddNewEntry()
    {
        lock (_statistics)
        {
            if (_statistics.Count == MaxHistoryLength)
            {
                _statistics.Dequeue();
            }

            _statistics.Enqueue(new Statistics<T>());
        }
    }

    public void UpdateLatestEntry(T value)
    {
        lock (_statistics)
        {
            _statistics.Last().Update(value);
        }
    }

    public void UpdateLatestEntry(Statistics<T> value)
    {
        lock (_statistics)
        {
            _statistics.Last().Update(value);
        }
    }

    public void Clear()
    {
        if (_statistics.Count == 0)
        {
            return;
        }

        lock (_statistics)
        {
            if (_statistics.Count == 0)
            {
                return;
            }

            _statistics.Clear();
        }
    }

    private T CalculateMin()
    {
        if (_statistics.Count == 0)
        {
            return default;
        }

        lock (_statistics)
        {
            if (_statistics.Count == 0)
            {
                return default;
            }

            return _statistics.Min(x => x.Min);
        }
    }

    private T CalculateAverage()
    {
        if (_statistics.Count == 0)
        {
            return default;
        }

        lock (_statistics)
        {
            if (_statistics.Count == 0)
            {
                return default;
            }

            T sum = default;
            T count = default;
            foreach (var statistic in _statistics)
            {
                sum += statistic.Average;
                count++;
            }

            return sum / count;
        }
    }

    private T CalculateMax()
    {
        if (_statistics.Count == 0)
        {
            return default;
        }

        lock (_statistics)
        {
            if (_statistics.Count == 0)
            {
                return default;
            }

            return _statistics.Max(x => x.Max);
        }
    }
}
