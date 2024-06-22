using System.Numerics;
using System.Runtime.CompilerServices;
using static Rays._3D.AxisAlignedBox;

namespace Rays._3D;

public sealed class VectorizedTriangleTree : ITriangleSetIntersector
{
    private readonly AxisAlignedBox[] _nodeBoundingBoxes;
    private readonly NodeInformation[] _nodeInformation;
    private readonly ITriangleSetIntersector[][] _nodeTexturedTriangles;
    private readonly CombinedTriangleTreeStatistics _treeStatistics;

    internal IEnumerable<(AxisAlignedBox Box, NodeInformation NodeInformation)> Nodes => _nodeBoundingBoxes.Zip(_nodeInformation);

    internal VectorizedTriangleTree(AxisAlignedBox[] nodeBoundingBoxes,
                                    NodeInformation[] nodeInformation,
                                    ITriangleSetIntersector[][] nodeTexturedTriangles,
                                    CombinedTriangleTreeStatistics treeStatistics)
    {
        _nodeBoundingBoxes = nodeBoundingBoxes;
        _nodeInformation = nodeInformation;
        _nodeTexturedTriangles = nodeTexturedTriangles;
        _treeStatistics = treeStatistics;
    }

    [SkipLocalsInit]
    public void TryGetIntersections(ReadOnlySpan<Ray> rays, Span<bool> raysHit, Span<(TriangleIntersection intersection, Color color)> triangleIntersections)
    {
        const int maxDepth = 40;
        Span<(int, float)> defaultNodeSpace = stackalloc (int, float)[maxDepth * rays.Length];
        Span<int> sdf = stackalloc int[rays.Length];
        sdf.Fill(0);
        var nodesToCheck = new SpanStackStack<(int, float)>(defaultNodeSpace, sdf, maxDepth);


        Span<RayAxisAlignBoxOptimizedIntersection> optimizedRayBoxIntersections = stackalloc RayAxisAlignBoxOptimizedIntersection[rays.Length];
        Span<bool> isCalculatingRayForNode = stackalloc bool[rays.Length];
        int rayCount = rays.Length;
        for (int i = 0; i < rays.Length; i++)
        {
            optimizedRayBoxIntersections[i] = new RayAxisAlignBoxOptimizedIntersection(rays[i]);
            if (_nodeBoundingBoxes[0].Intersects(optimizedRayBoxIntersections[i]))
            {
                nodesToCheck.Push((0, 0), i);
            }
            else
            {
                rayCount--;
            }
        }

        if (rayCount == 0)
        {
            raysHit.Fill(false);
            return;
        }


        Span<float> bestDistances = stackalloc float[rays.Length];
        bestDistances.Fill(float.MaxValue);
        while (nodesToCheck.AcrossSpanCount > 0)
        {
            int nodeRaysToCalculate = rays.Length;
            for (int i = 0; i < rays.Length; i++)
            {
                if (nodesToCheck.GetCount(i) > 0)
                {
                    (int nodeIndex, float nodeDistance) = nodesToCheck.Peek(i);
                    if (nodeDistance > bestDistances[i])
                    {
                        isCalculatingRayForNode[i] = false;
                        nodesToCheck.PopNoReturn(i);
                        nodeRaysToCalculate--;
                        continue;
                    }
                }
                else
                {
                    isCalculatingRayForNode[i] = false;
                    nodeRaysToCalculate--;
                    continue;
                }

                isCalculatingRayForNode[i] = true;
            }

            if (nodeRaysToCalculate == 0)
            {
                continue;
            }

            for (int i = 0; i < rays.Length; i++)
            {
                if (!isCalculatingRayForNode[i])
                {
                    continue;
                }

                (int nodeIndex, float nodeDistance) = nodesToCheck.Peek(i);
                NodeInformation nodeInformation = _nodeInformation[nodeIndex];
                if (nodeInformation.IsLeafNode)
                {
                    isCalculatingRayForNode[i] = false;
                    nodesToCheck.PopNoReturn(i);

                    ref readonly Ray ray = ref rays[i];
                    if (TryGetIntersectionWithTriangles(ray, _nodeTexturedTriangles[nodeInformation.TexturedTriangleIndex], out var intersection))
                    {
                        float distance = Vector4.Distance(ray.Start, intersection.intersection.GetIntersection(ray));
                        if (distance > bestDistances[i])
                        {
                            continue;
                        }

                        bestDistances[i] = distance;
                        triangleIntersections[i] = intersection;
                    }

                    // A node either contains nodes or triangles, never both.
                    continue;
                }
            }

            if (nodeRaysToCalculate == 0)
            {
                continue;
            }

            for (int i = 0; i < rays.Length; i++)
            {
                if (!isCalculatingRayForNode[i])
                {
                    continue;
                }

                (int nodeIndex, float nodeDistance) = nodesToCheck.Pop(i);
                NodeInformation nodeInformation = _nodeInformation[nodeIndex];

                int nodeChildStartIndex = nodeInformation.ChildStartIndex;
                int nodeChildCount = nodeInformation.ChildCount;
                for (int nodeChildIndex = 0; nodeChildIndex < nodeChildCount; nodeChildIndex++)
                {
                    int childIndex = nodeChildStartIndex + nodeChildIndex;
                    ref readonly AxisAlignedBox childBoundingBox = ref _nodeBoundingBoxes[childIndex];
                    if (childBoundingBox.Intersects(optimizedRayBoxIntersections[i]))
                    {
                        float distance = Vector4.Distance(rays[i].Start, childBoundingBox.Center) - childBoundingBox.Size.Length();
                        nodesToCheck.Push((childIndex, distance), i);
                    }
                }
            }
        }

        for (int i = 0; i < rays.Length; i++)
        {
            raysHit[i] = bestDistances[i] != float.MaxValue;
        }
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection) => throw new NotImplementedException();

    private static bool TryGetIntersectionWithTriangles(Ray ray, ITriangleSetIntersector[] texturedTriangleSets, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            if (!texturedTriangles.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
            {
                continue;
            }

            float distance = Vector4.DistanceSquared(ray.Start, triangleIntersection.intersection.GetIntersection(ray));
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

    private ref struct SpanStackStack<T> where T : struct
    {
        private Span<T> _items;
        private Span<int> _counts;
        private readonly int _maxStackDepth;
        private int _acrossSpanCount;

        public readonly int AcrossSpanCount => _acrossSpanCount;

        public SpanStackStack(Span<T> itemsContainer, Span<int> counts, int maxStackDepth)
        {
            _items = itemsContainer;
            _counts = counts;
            _maxStackDepth = maxStackDepth;
        }

        public int GetCount(int index)
        {
            return _counts[index];
        }

        public void Push(T item, int index)
        {
            _acrossSpanCount++;
            _items[_maxStackDepth * index + _counts[index]++] = item;
        }

        public T Pop(int index)
        {
            _acrossSpanCount--;
            return _items[_maxStackDepth * index + --_counts[index]];
        }

        public void PopNoReturn(int index)
        {
            _acrossSpanCount--;
            _counts[index]--;
        }

        public T Peek(int index)
        {
            return _items[_maxStackDepth * index + _counts[index] - 1];
        }
    }
}
