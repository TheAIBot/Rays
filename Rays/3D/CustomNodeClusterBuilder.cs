using System.Numerics;

namespace Rays._3D;

public sealed class CustomNodeClusterBuilder : INodeClusterBuilder
{
    public Node Create(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        AxisAlignedBox rootBox = AxisAlignedBox.GetBoundingBoxForTriangles(texturedTriangleSets.SelectMany(x => x.GetTriangles()));
        var root = new Node(texturedTriangleSets, new List<Node>());
        var nodesToGoThrough = new Stack<(Node, AxisAlignedBox)>();
        nodesToGoThrough.Push((root, rootBox));

        while (nodesToGoThrough.Count > 0)
        {
            (var node, var boundingBox) = nodesToGoThrough.Pop();

            int trianglesInParent = node.TexturedTriangleSets.Sum(x => x.Triangles.Length);
            var children = new List<Node>();
            var wantToSplitUp = new List<(Node, AxisAlignedBox)>();
            foreach (var childBox in Get8SubBoxes(rootBox))
            {
                ISubDividableTriangleSet[] childTexturedTriangleSet = node.TexturedTriangleSets
                                                                    .Select(x => x.SubCopy(y => childBox.CollidesWith(y)))
                                                                    .Where(x => x.Triangles.Length > 0)
                                                                    .ToArray();
                int trianglesInChild = childTexturedTriangleSet.Sum(x => x.Triangles.Length);
                if (trianglesInChild == 0)
                {
                    continue;
                }

                AxisAlignedBox fullSizedBox = AxisAlignedBox.GetBoundingBoxForTriangles(childTexturedTriangleSet.SelectMany(x => x.GetTriangles()));
                AxisAlignedBox noLargerThanChildBox = new AxisAlignedBox(Vector4.Max(childBox.MinPosition, fullSizedBox.MinPosition), Vector4.Min(childBox.MaxPosition, fullSizedBox.MaxPosition));
                var childNode = new Node(childTexturedTriangleSet, new List<Node>());
                children.Add(childNode);

                const int minTrianglesPerNode = 50;
                if (trianglesInChild < trianglesInParent && trianglesInChild > minTrianglesPerNode)
                {
                    wantToSplitUp.Add((childNode, noLargerThanChildBox));
                }
            }

            if (TooManyDuplicateTriangles(children))
            {
                continue;
            }

            node.Children.AddRange(children);
            foreach (var nodeToSplitUp in wantToSplitUp)
            {
                nodesToGoThrough.Push(nodeToSplitUp);
            }
        }

        return root;
    }

    private static bool TooManyDuplicateTriangles(List<Node> children)
    {
        int childTotalTriangleCount = children.Sum(x => x.TexturedTriangleSets.Sum(y => y.Triangles.Length));
        int UniqueTrianglesInChildrenCount = children.SelectMany(x => x.TexturedTriangleSets.SelectMany(y => y.Triangles)).Distinct().Count();
        float duplicateTrianglesRatio = 1.0f - (UniqueTrianglesInChildrenCount / (float)childTotalTriangleCount);
        const float maxAllowedDuplicateTrianglesRatio = 0.4f;

        return duplicateTrianglesRatio > maxAllowedDuplicateTrianglesRatio;
    }

    private static AxisAlignedBox[] Get8SubBoxes(AxisAlignedBox box)
    {
        static AxisAlignedBox CreateBox(Vector4 minPosition, Vector4 boxSize, int x, int y, int z)
        {
            // There is rare cases of 8 sub boxes not covering the entire area that the
            // large box did because of floating point error which is why the boxes will
            // overlap slightly.
            Vector4 smallOverlap = new Vector4(0.00001f);
            var axisMove = new Vector4(x, y, z, 0);
            var boxMinPosition = minPosition + axisMove * boxSize;
            return new AxisAlignedBox(boxMinPosition - smallOverlap, boxMinPosition + boxSize + smallOverlap);
        }

        Vector4 halfSize = Vector4.Abs((box.MaxPosition - box.MinPosition) * 0.5f);
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
