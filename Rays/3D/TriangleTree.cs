using System.Collections.Concurrent;
using System.Numerics;
using static Rays._3D.AxisAlignedBox;
using static Rays._3D.Triangle;

namespace Rays._3D;

public sealed class TriangleTree : ITriangleSetIntersector
{
    private readonly Node[] _nodes;
    private readonly ITriangleSetIntersector[][] _nodeTexturedTriangles;
    private readonly CombinedTriangleTreeStatistics _treeStatistics;

    internal TriangleTree(Node[] nodes, ITriangleSetIntersector[][] nodeTexturedTriangles, CombinedTriangleTreeStatistics treeStatistics)
    {
        _nodes = nodes;
        _nodeTexturedTriangles = nodeTexturedTriangles;
        _treeStatistics = treeStatistics;
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        var rayTriangleOptimizedIntersection = new RayTriangleOptimizedIntersection(ray);
        return TryGetIntersection(rayTriangleOptimizedIntersection, out triangleIntersection);
    }

    public bool TryGetIntersection(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        var optimizedRayBoxIntersection = new RayAxisAlignBoxOptimizedIntersection(rayTriangleOptimizedIntersection);

        triangleIntersection = default;
        if (!_nodes[0].BoundingBox.Intersects(optimizedRayBoxIntersection))
        {
            return false;
        }

        float bestDistance = float.MaxValue;
        var statistics = new TriangleTreeStatistics();

        var nodesToCheck = new PriorityQueue<int, float>();
        nodesToCheck.Enqueue(0, 0);
        while (nodesToCheck.Count > 0)
        {
            Node node = _nodes[nodesToCheck.Dequeue()];
            statistics.NodesTraversed++;
            if (node.ContainsTriangles)
            {
                if (TryGetIntersectionWithTriangles(ref statistics, rayTriangleOptimizedIntersection, _nodeTexturedTriangles[node.TexturedTrianglesIndex], out var intersection))
                {
                    float distance = Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, intersection.intersection.GetIntersection(rayTriangleOptimizedIntersection));
                    if (distance > bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    triangleIntersection = intersection;
                }

                // A node either contains nodes or triangles, never both.
                continue;
            }


            var nodeChildren = node.Children.GetAsSpan(_nodes);
            for (int i = 0; i < nodeChildren.Length; i++)
            {
                Node child = nodeChildren[i];
                if (child.BoundingBox.Intersects(optimizedRayBoxIntersection))
                {
                    float distance = Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, new Vector4(child.BoundingBox.Center, 0));
                    nodesToCheck.Enqueue(node.Children.Index + i, distance);
                }
            }
        }

        _treeStatistics.AddStatistic(statistics);
        return bestDistance != float.MaxValue;
    }

    private bool TryGetIntersectionWithTriangles(ref TriangleTreeStatistics statistics, RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection, ITriangleSetIntersector[] texturedTriangleSets, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            statistics.TrianglesChecked += texturedTriangles.GetTriangles().TryGetNonEnumeratedCount(out int count) ? count : texturedTriangles.GetTriangles().Count();
            if (!texturedTriangles.TryGetIntersection(rayTriangleOptimizedIntersection, out (TriangleIntersection intersection, Color color) triangleIntersection))
            {
                continue;
            }

            statistics.IntersectionsFound++;
            float distance = Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, triangleIntersection.intersection.GetIntersection(rayTriangleOptimizedIntersection));
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            intersection = triangleIntersection;
        }

        return bestDistance != float.MaxValue;
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return _nodeTexturedTriangles.SelectMany(x => x.SelectMany(y => y.GetTriangles()));
    }

    public readonly record struct Node(AxisAlignedBox BoundingBox, SpanRange Children, int TexturedTrianglesIndex)
    {
        public bool ContainsTriangles => TexturedTrianglesIndex != -1;
    }

    public readonly record struct SpanRange(int Index, int Length)
    {
        public Span<T> GetAsSpan<T>(T[] array)
        {
            return array.AsSpan(Index, Length);
        }
    }
}

public record struct TriangleTreeStatistics(float NodesTraversed, float TrianglesChecked, float IntersectionsFound);

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

public readonly record struct HistoricalStatistics<T>(int MaxHistoryLength) where T : struct, INumber<T>
{
    private readonly Queue<Statistics<T>> _statistics = new Queue<Statistics<T>>();

    public T Min => CalculateMin();
    public T Average => CalculateAverage();
    public T Max => CalculateMax();

    public void AddNewEntry()
    {
        if (_statistics.Count == MaxHistoryLength)
        {
            _statistics.Dequeue();
        }

        _statistics.Enqueue(new Statistics<T>());
    }

    public void UpdateLatestEntry(T value)
    {
        _statistics.Last().Update(value);
    }

    public void Clear()
    {
        _statistics.Clear();
    }

    private T CalculateMin()
    {
        if (_statistics.Count == 0)
        {
            return default;
        }

        return _statistics.Min(x => x.Min);
    }

    private T CalculateAverage()
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

    private T CalculateMax()
    {
        if (_statistics.Count == 0)
        {
            return default;
        }

        return _statistics.Max(x => x.Max);
    }
}


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