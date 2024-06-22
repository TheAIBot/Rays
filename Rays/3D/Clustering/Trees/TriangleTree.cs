using System.Numerics;
using static Rays._3D.AxisAlignedBox;

namespace Rays._3D;

public sealed class TriangleTree : ITriangleSetIntersector
{
    private readonly AxisAlignedBox[] _nodeBoundingBoxes;
    private readonly NodeInformation[] _nodeInformation;
    private readonly ITriangleSetIntersector[][] _nodeTexturedTriangles;
    private readonly CombinedTriangleTreeStatistics _treeStatistics;

    internal IEnumerable<(AxisAlignedBox Box, NodeInformation NodeInformation)> Nodes => _nodeBoundingBoxes.Zip(_nodeInformation);

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

    public void TryGetIntersections(ReadOnlySpan<Ray> rays, Span<bool> raysHit, Span<(TriangleIntersection intersection, Color color)> triangleIntersections)
    {
        for (int i = 0; i < rays.Length; i++)
        {
            raysHit[i] = TryGetIntersection(rays[i], out triangleIntersections[i]);
        }
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        var optimizedRayBoxIntersection = new RayAxisAlignBoxOptimizedIntersection(ray);

        triangleIntersection = default;
        if (!_nodeBoundingBoxes[0].Intersects(optimizedRayBoxIntersection))
        {
            return false;
        }

        float bestDistance = float.MaxValue;
        var statistics = new TriangleTreeStatistics();

        Span<(int, float)> defaultNodeSpace = stackalloc (int, float)[40];
        var nodesToCheck = new StackStack<(int, float)>(defaultNodeSpace);
        nodesToCheck.Push((0, 0));
        while (nodesToCheck.Count > 0)
        {
            (int nodeIndex, float nodeDistance) = nodesToCheck.Pop();
            if (nodeDistance > bestDistance)
            {
                continue;
            }

            NodeInformation nodeInformation = _nodeInformation[nodeIndex];
            statistics.NodesTraversed++;
            if (nodeInformation.IsLeafNode)
            {
                if (TryGetIntersectionWithTriangles(ref statistics, ray, _nodeTexturedTriangles[nodeInformation.TexturedTriangleIndex], out var intersection))
                {
                    float distance = Vector4.Distance(ray.Start, intersection.intersection.GetIntersection(ray));
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
                    float distance = Vector4.Distance(ray.Start, childBoundingBox.Center) - childBoundingBox.Size.Length();
                    nodesToCheck.Push((childIndex, distance));
                }
            }
        }

        _treeStatistics.AddStatistic(statistics);
        return bestDistance != float.MaxValue;
    }

    private static bool TryGetIntersectionWithTriangles(ref TriangleTreeStatistics statistics, Ray ray, ITriangleSetIntersector[] texturedTriangleSets, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            statistics.TrianglesChecked += texturedTriangles.GetTriangles().TryGetNonEnumeratedCount(out int count) ? count : texturedTriangles.GetTriangles().Count();
            if (!texturedTriangles.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
            {
                continue;
            }

            statistics.IntersectionsFound++;
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

    private ref struct StackStack<T> where T : struct
    {
        private Span<T> _items;
        private int _count;

        public readonly int Count => _count;

        public StackStack(Span<T> itemsContainer)
        {
            _items = itemsContainer;
        }

        public void Push(T item)
        {
            if (_count >= _items.Length)
            {
                Span<T> largerItemContainer = new T[_items.Length * 2];
                _items.CopyTo(largerItemContainer);
                _items = largerItemContainer;
            }

            _items[_count++] = item;
        }

        public T Pop()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("No items left to pop of the stack.");
            }

            return _items[--_count];
        }
    }
}