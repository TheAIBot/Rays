using System.Collections.Concurrent;

namespace Rays._3D;

public sealed class CombinedTriangleTreeStatistics
{
    private readonly ConcurrentQueue<TriangleTreeStatistics>[] _unprocessedStatistics;

    public HistoricalStatistics<float> NodesTraversed { get; } = new HistoricalStatistics<float>(20);
    public HistoricalStatistics<float> TrianglesChecked { get; } = new HistoricalStatistics<float>(20);
    public HistoricalStatistics<float> IntersectionsFound { get; } = new HistoricalStatistics<float>(20);

    public CombinedTriangleTreeStatistics()
    {
        _unprocessedStatistics = new ConcurrentQueue<TriangleTreeStatistics>[Environment.ProcessorCount];
        for (int i = 0; i < _unprocessedStatistics.Length; i++)
        {
            _unprocessedStatistics[i] = new ConcurrentQueue<TriangleTreeStatistics>();
        }
    }

    public void AddStatistic(TriangleTreeStatistics statistics)
    {
        _unprocessedStatistics[Environment.CurrentManagedThreadId % _unprocessedStatistics.Length].Enqueue(statistics);
    }

    public void ProcessStatistics()
    {
        NodesTraversed.AddNewEntry();
        TrianglesChecked.AddNewEntry();
        IntersectionsFound.AddNewEntry();

        foreach (var unprocessedStatistics in _unprocessedStatistics)
        {
            int itemCount = unprocessedStatistics.Count;
            for (int i = 0; i < itemCount; i++)
            {
                unprocessedStatistics.TryDequeue(out TriangleTreeStatistics statistics);

                NodesTraversed.UpdateLatestEntry(statistics.NodesTraversed);
                TrianglesChecked.UpdateLatestEntry(statistics.TrianglesChecked);
                IntersectionsFound.UpdateLatestEntry(statistics.IntersectionsFound);
            }
        }
    }

    public void Clear()
    {
        NodesTraversed.Clear();
        TrianglesChecked.Clear();
        IntersectionsFound.Clear();
    }
}
