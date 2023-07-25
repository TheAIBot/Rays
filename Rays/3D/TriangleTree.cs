﻿using System.Numerics;
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
            int nodeIndex;
            float nodeDistance;
            nodesToCheck.TryDequeue(out nodeIndex, out nodeDistance);
            if (nodeDistance > bestDistance)
            {
                continue;
            }

            NodeInformation nodeInformation = _nodeInformation[nodeIndex];
            statistics.NodesTraversed++;
            if (nodeInformation.IsLeafNode)
            {
                if (TryGetIntersectionWithTriangles(ref statistics, rayTriangleOptimizedIntersection, _nodeTexturedTriangles[nodeInformation.TexturedTriangleIndex], out var intersection))
                {
                    float distance = ManhattanDistance(rayTriangleOptimizedIntersection.Start, intersection.intersection.GetIntersection(rayTriangleOptimizedIntersection));
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
                    float distance = ManhattanDistance(Vector4.Abs(rayTriangleOptimizedIntersection.Start - childBoundingBox.Center), childBoundingBox.Size);
                    nodesToCheck.Enqueue(childIndex, distance);
                }
            }
        }

        _treeStatistics.AddStatistic(statistics);
        return bestDistance != float.MaxValue;
    }

    private static float ManhattanDistance(Vector4 a, Vector4 b)
    {
        Vector4 distance = Vector4.Abs(a - b);
        return Vector4.Dot(distance, Vector4.One);
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
