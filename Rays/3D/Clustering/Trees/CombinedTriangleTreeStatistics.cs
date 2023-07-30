using System.Collections.Concurrent;

namespace Rays._3D;

public sealed class CombinedTriangleTreeStatistics
{
    private readonly ConcurrentQueue<TriangleTreeStatistics> _unprocessedStatistics = new ConcurrentQueue<TriangleTreeStatistics>();

    public HistoricalStatistics<float> NodesTraversed { get; } = new HistoricalStatistics<float>(20);
    public HistoricalStatistics<float> TrianglesChecked { get; } = new HistoricalStatistics<float>(20);
    public HistoricalStatistics<float> IntersectionsFound { get; } = new HistoricalStatistics<float>(20);

    public void AddStatistic(TriangleTreeStatistics statistics)
    {
        _unprocessedStatistics.Enqueue(statistics);
    }

    public void ProcessStatistics()
    {
        NodesTraversed.AddNewEntry();
        TrianglesChecked.AddNewEntry();
        IntersectionsFound.AddNewEntry();

        int itemCount = _unprocessedStatistics.Count;
        for (int i = 0; i < itemCount; i++)
        {
            TriangleTreeStatistics statistics;
            _unprocessedStatistics.TryDequeue(out statistics);

            NodesTraversed.UpdateLatestEntry(statistics.NodesTraversed);
            TrianglesChecked.UpdateLatestEntry(statistics.TrianglesChecked);
            IntersectionsFound.UpdateLatestEntry(statistics.IntersectionsFound);
        }
    }

    public void Clear()
    {
        NodesTraversed.Clear();
        TrianglesChecked.Clear();
        IntersectionsFound.Clear();
    }
}
