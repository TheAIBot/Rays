﻿using System.Numerics;

namespace Rays._3D;

public static class TriangleTreeBuilder
{
    internal const int MaxChildCount = 8;

    public static TriangleTree Create(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        const int maxTrianglesPerLeaf = 100;
        AxisAlignedBox rootBox = GetBoundingBox(texturedTriangleSets);
        Node root = new Node(rootBox, texturedTriangleSets, new List<Node>());
        Stack<Node> nodesToGoThrough = new Stack<Node>();
        nodesToGoThrough.Push(root);

        while (nodesToGoThrough.Count > 0)
        {
            Node node = nodesToGoThrough.Pop();
            if (node.TexturedTriangleSets.Sum(x => x.Triangles.Length) <= maxTrianglesPerLeaf)
            {
                continue;
            }

            foreach (var childBox in Get8SubBoxes(node.BoundingBox))
            {
                ISubDividableTriangleSet[] childTexturedTriangleSet = node.TexturedTriangleSets
                                                                    .Select(x => x.SubCopy(y => childBox.CollidesWith(y)))
                                                                    .Where(x => x.Triangles.Length > 0)
                                                                    .ToArray();
                if (childTexturedTriangleSet.Sum(x => x.Triangles.Length) == 0)
                {
                    continue;
                }

                AxisAlignedBox fullSizedBox = GetBoundingBox(childTexturedTriangleSet);
                Vector3 childBoxSize = Vector3.Abs(childBox.MaxPosition - childBox.MinPosition);
                float childBoxVolume = childBoxSize.X * childBoxSize.Y * childBoxSize.Z;
                Vector3 fulLSizeBoxSize = Vector3.Abs(fullSizedBox.MaxPosition - fullSizedBox.MinPosition);
                float fullSizeBoxVolume = fulLSizeBoxSize.X * fulLSizeBoxSize.Y * fulLSizeBoxSize.Z;
                var childNode = new Node(fullSizedBox, childTexturedTriangleSet, new List<Node>());
                node.Children.Add(childNode);

                // Only add child node if the node is small enough. If it's not small enough
                // then splitting it up further will not help any further.
                if (fullSizeBoxVolume <= childBoxVolume * 4)
                {
                    nodesToGoThrough.Push(childNode);
                }
            }
        }

        TriangleTree.Node[] treeNodes = new TriangleTree.Node[root.CountNodes()];
        ISubDividableTriangleSet[][] triangleSets = new ISubDividableTriangleSet[root.CountNodes(x => x.Children.Count == 0)][];
        int texturedSetsIndex = 0;
        int lastNodeIndex = treeNodes.Length - 1;
        Dictionary<Node, int> nodeToIndex = new Dictionary<Node, int>();
        foreach (var node in root.ReverseBreadthFirstOrder())
        {
            if (node.Children.Count == 0)
            {
                triangleSets[texturedSetsIndex] = node.TexturedTriangleSets;
                treeNodes[lastNodeIndex] = new TriangleTree.Node(node.BoundingBox, default, texturedSetsIndex);
                nodeToIndex.Add(node, lastNodeIndex);
                lastNodeIndex--;
                texturedSetsIndex++;
            }
            else
            {
                int firstChildIndex = nodeToIndex[node.Children[0]];
                treeNodes[lastNodeIndex] = new TriangleTree.Node(node.BoundingBox, new TriangleTree.SpanRange(firstChildIndex, node.Children.Count), -1);
                nodeToIndex.Add(node, lastNodeIndex);
                lastNodeIndex--;
            }
        }

        return new TriangleTree(treeNodes, triangleSets);
    }

    private sealed record Node(AxisAlignedBox BoundingBox, ISubDividableTriangleSet[] TexturedTriangleSets, List<Node> Children)
    {
        public int CountNodes()
        {
            return BreadthFirstOrder().Count();
        }

        public int CountNodes(Func<Node, bool> filter)
        {
            return BreadthFirstOrder().Where(filter).Count();
        }

        public IEnumerable<Node> BreadthFirstOrder()
        {
            Queue<Node> nodesToGoThrough = new Queue<Node>();
            nodesToGoThrough.Enqueue(this);
            while (nodesToGoThrough.Count > 0)
            {
                Node node = nodesToGoThrough.Dequeue();
                yield return node;
                foreach (var child in node.Children)
                {
                    nodesToGoThrough.Enqueue(child);
                }
            }
        }

        public IEnumerable<Node> ReverseBreadthFirstOrder()
        {
            return BreadthFirstOrder().Reverse<Node>();
        }
    }

    private static AxisAlignedBox GetBoundingBox(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        return new AxisAlignedBox(GetMinPoint(texturedTriangleSets), GetMaxPoint(texturedTriangleSets));
    }

    private static Vector3 GetMinPoint(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            foreach (var triangle in texturedTriangles.Triangles)
            {
                min = Vector3.Min(min, triangle.CornerA);
                min = Vector3.Min(min, triangle.CornerB);
                min = Vector3.Min(min, triangle.CornerC);
            }
        }

        return min;
    }

    private static Vector3 GetMaxPoint(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            foreach (var triangle in texturedTriangles.Triangles)
            {
                max = Vector3.Max(max, triangle.CornerA);
                max = Vector3.Max(max, triangle.CornerB);
                max = Vector3.Max(max, triangle.CornerC);
            }
        }

        return max;
    }

    private static AxisAlignedBox[] Get8SubBoxes(AxisAlignedBox box)
    {
        static AxisAlignedBox CreateBox(Vector3 minPosition, Vector3 boxSize, int x, int y, int z)
        {
            // There is rare cases of 8 sub boxes not covering the entire area that the
            // large box did because of floating point error which is why the boxes will
            // overlap slightly.
            Vector3 smallOverlap = new Vector3(0.00001f);
            var axisMove = new Vector3(x, y, z);
            var boxMinPosition = minPosition + axisMove * boxSize;
            return new AxisAlignedBox(boxMinPosition - smallOverlap, boxMinPosition + boxSize + smallOverlap);
        }

        Vector3 halfSize = Vector3.Abs((box.MaxPosition - box.MinPosition) * 0.5f);
        return new AxisAlignedBox[]
        {
            CreateBox(box.MinPosition, halfSize, 0, 0, 0),
            CreateBox(box.MinPosition, halfSize, 0, 0, 1),
            CreateBox(box.MinPosition, halfSize, 0, 1, 0),
            CreateBox(box.MinPosition, halfSize, 0, 1, 1),
            CreateBox(box.MinPosition, halfSize, 1, 0, 0),
            CreateBox(box.MinPosition, halfSize, 1, 0, 1),
            CreateBox(box.MinPosition, halfSize, 1, 1, 0),
            CreateBox(box.MinPosition, halfSize, 1, 1, 1),
        };
    }
}