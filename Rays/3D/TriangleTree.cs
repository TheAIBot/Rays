using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using static Rays._3D.AxisAlignedBox;
using static Rays._3D.Triangle;

namespace Rays._3D;

public sealed class TriangleTree : ITriangleSetIntersector
{
    private readonly AxisAlignedBox[] _nodeBoundingBoxes;
    private readonly NodeInformation[] _nodeInformation;
    private readonly ITriangleSetIntersector[][] _nodeTexturedTriangles;
    private readonly CombinedTriangleTreeStatistics _treeStatistics;



    internal TriangleTree(AxisAlignedBox[] nodeBoundingBoxes,
                          NodeInformation[] nodeInformation,
                          ITriangleSetIntersector[][] nodeTexturedTriangles,
                          CombinedTriangleTreeStatistics treeStatistics)
    {
        _nodeBoundingBoxes = nodeBoundingBoxes;
        _nodeInformation = nodeInformation;
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
        if (!_nodeBoundingBoxes[0].Intersects(optimizedRayBoxIntersection))
        {
            return false;
        }

        float bestDistance = float.MaxValue;
        var statistics = new TriangleTreeStatistics();

        var nodesToCheck = new PriorityQueue<int, float>();
        nodesToCheck.Enqueue(0, 0);
        while (nodesToCheck.Count > 0)
        {
            int nodeIndex = nodesToCheck.Dequeue();
            NodeInformation nodeInformation = _nodeInformation[nodeIndex];
            statistics.NodesTraversed++;
            if (nodeInformation.IsLeafNode)
            {
                if (TryGetIntersectionWithTriangles(ref statistics, rayTriangleOptimizedIntersection, _nodeTexturedTriangles[nodeInformation.TexturedTriangleIndex], out var intersection))
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

            int nodeChildStartIndex = nodeInformation.ChildStartIndex;
            int nodeChildCount = nodeInformation.ChildCount;
            for (int i = 0; i < nodeChildCount; i++)
            {
                int childIndex = nodeChildStartIndex + i;
                ref readonly AxisAlignedBox childBoundingBox = ref _nodeBoundingBoxes[childIndex];
                if (childBoundingBox.Intersects(optimizedRayBoxIntersection))
                {
                    float distance = Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, childBoundingBox.Center);
                    nodesToCheck.Enqueue(childIndex, distance);
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

    public readonly struct NodeInformation
    {
        private const uint _isLeafNodeMask = 0b10000000_00000000_00000000_00000000;
        private const int _isLeafNodeShift = 31;
        private const uint _childCountMask = 0b01111111_00000000_00000000_00000000;
        private const int _childCountShift = 24;
        private const uint _childStartMask = 0b00000000_11111111_11111111_11111111;
        private const int _childStartShift = 0;
        private const uint _trianglesMask_ = 0b01111111_11111111_11111111_11111111;
        private const int _trianglesShift_ = 0;
        private readonly uint _value;

        public bool IsLeafNode => ((_value & _isLeafNodeMask) >> _isLeafNodeShift) == 1;

        public int ChildStartIndex => (int)((_value & _childStartMask) >> _childStartShift);

        public int ChildCount => (int)((_value & _childCountMask) >> _childCountShift);

        public int TexturedTriangleIndex => (int)((_value & _trianglesMask_) >> _trianglesShift_);

        public static NodeInformation CreateLeafNode(int texturedTrianglesIndex)
        {
            return new NodeInformation(true, 0, 0, texturedTrianglesIndex);
        }

        public static NodeInformation CreateParentNode(int childStartIndex, int childCount)
        {
            return new NodeInformation(false, childStartIndex, childCount, 0);
        }

        private NodeInformation(bool isLeafNode, int childStartIndex, int childCount, int texturedTrianglesIndex)
        {
            WithinBounds(childStartIndex, _childStartMask, _childStartShift);
            WithinBounds(childCount, _childCountMask, _childCountShift);
            WithinBounds(texturedTrianglesIndex, _trianglesMask_, _trianglesShift_);

            _value = (isLeafNode ? 1u : 0u) << _isLeafNodeShift;
            _value |= ((uint)childCount << _childCountShift) & _childCountMask;
            _value |= ((uint)childStartIndex << _childStartShift) & _childStartMask;
            _value |= ((uint)texturedTrianglesIndex << _trianglesShift_) & _trianglesMask_;
        }

        private static void WithinBounds(int value, uint mask, int shift, [CallerArgumentExpression(nameof(value))] string? name = null)
        {
            int maxValue = (int)(mask >> shift);
            if (value > maxValue || value < 0)
            {
                throw new ArgumentOutOfRangeException(name, $"Value must be within 0 to {maxValue} but had value {value}.");
            }
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