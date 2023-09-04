namespace Rays._3D;

public sealed class CombinedTriangleTreeStatistics : IDisposable
{
    private readonly ThreadLocal<ThreadAccumulatedStatistics> _perThreadStatistics = new(() =>
    {
        return new ThreadAccumulatedStatistics(new Statistics<float>(), new Statistics<float>(), new Statistics<float>());
    }, true);

    public HistoricalStatistics<float> NodesTraversed { get; } = new HistoricalStatistics<float>(20);
    public HistoricalStatistics<float> TrianglesChecked { get; } = new HistoricalStatistics<float>(20);
    public HistoricalStatistics<float> IntersectionsFound { get; } = new HistoricalStatistics<float>(20);

    public void AddStatistic(TriangleTreeStatistics statistic)
    {
        ThreadAccumulatedStatistics? threadStatistics = _perThreadStatistics.Value;
        if (threadStatistics == null)
        {
            throw new InvalidOperationException("Thread statistics was null");
        }

        threadStatistics.NodesTraversed.Update(statistic.NodesTraversed);
        threadStatistics.TrianglesChecked.Update(statistic.TrianglesChecked);
        threadStatistics.IntersectionsFound.Update(statistic.IntersectionsFound);
    }

    public void ProcessStatistics()
    {
        NodesTraversed.AddNewEntry();
        TrianglesChecked.AddNewEntry();
        IntersectionsFound.AddNewEntry();

        foreach (var unprocessedStatistics in _perThreadStatistics.Values)
        {
            NodesTraversed.UpdateLatestEntry(unprocessedStatistics.NodesTraversed);
            TrianglesChecked.UpdateLatestEntry(unprocessedStatistics.TrianglesChecked);
            IntersectionsFound.UpdateLatestEntry(unprocessedStatistics.IntersectionsFound);
            unprocessedStatistics.NodesTraversed.Clear();
            unprocessedStatistics.TrianglesChecked.Clear();
            unprocessedStatistics.IntersectionsFound.Clear();
        }
    }

    public void Clear()
    {
        NodesTraversed.Clear();
        TrianglesChecked.Clear();
        IntersectionsFound.Clear();
    }

    public void Dispose()
    {
        _perThreadStatistics.Dispose();
    }

    private sealed record ThreadAccumulatedStatistics(Statistics<float> NodesTraversed, Statistics<float> TrianglesChecked, Statistics<float> IntersectionsFound);
}
