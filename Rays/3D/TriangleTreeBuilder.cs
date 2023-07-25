using System.Numerics;
using static Rays._3D.TriangleTree;

namespace Rays._3D;

public class TriangleTreeBuilder
{
    private readonly CombinedTriangleTreeStatistics _combinedTriangleTreeStatistics;

    public TriangleTreeBuilder(CombinedTriangleTreeStatistics combinedTriangleTreeStatistics)
    {
        _combinedTriangleTreeStatistics = combinedTriangleTreeStatistics;
    }

    public TriangleTree Create(ISubDividableTriangleSet[] texturedTriangleSets)
    {
        AxisAlignedBox rootBox = AxisAlignedBox.GetBoundingBoxForTriangles(texturedTriangleSets.SelectMany(x => x.GetTriangles()));
        Node root = new Node(null, rootBox, texturedTriangleSets, new List<Node>());
        Stack<Node> nodesToGoThrough = new Stack<Node>();
        nodesToGoThrough.Push(root);

        while (nodesToGoThrough.Count > 0)
        {
            Node node = nodesToGoThrough.Pop();

            int trianglesInParent = node.TexturedTriangleSets.Sum(x => x.Triangles.Length);
            foreach (var childBox in Get8SubBoxes(node.BoundingBox))
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
                var childNode = new Node(node, noLargerThanChildBox, childTexturedTriangleSet, new List<Node>());
                node.Children.Add(childNode);

                int minTrianglesPerNode = 20;
                if (trianglesInChild < trianglesInParent && trianglesInChild > minTrianglesPerNode)
                {
                    nodesToGoThrough.Push(childNode);
                }
            }
        }

        int nodeCount = root.CountNodes();
        AxisAlignedBox[] nodeBoundingBoxes = new AxisAlignedBox[nodeCount];
        NodeInformation[] nodeInformation = new NodeInformation[nodeCount];
        ISubDividableTriangleSet[][] triangleSets = new ISubDividableTriangleSet[root.CountNodes(x => x.Children.Count == 0)][];
        int texturedSetsIndex = 0;
        int lastNodeIndex = nodeCount - 1;
        Dictionary<Node, int> nodeToIndex = new Dictionary<Node, int>();
        foreach (var node in root.ReverseBreadthFirstOrder())
        {
            if (node.Children.Count == 0)
            {
                nodeBoundingBoxes[lastNodeIndex] = node.BoundingBox;
                nodeInformation[lastNodeIndex] = NodeInformation.CreateLeafNode(texturedSetsIndex);
                triangleSets[texturedSetsIndex] = node.TexturedTriangleSets;
                nodeToIndex.Add(node, lastNodeIndex);
                lastNodeIndex--;
                texturedSetsIndex++;
            }
            else
            {
                int firstChildIndex = nodeToIndex[node.Children[0]];
                nodeBoundingBoxes[lastNodeIndex] = node.BoundingBox;
                nodeInformation[lastNodeIndex] = NodeInformation.CreateParentNode(firstChildIndex, node.Children.Count);
                nodeToIndex.Add(node, lastNodeIndex);
                lastNodeIndex--;
            }
        }

        return new TriangleTree(nodeBoundingBoxes, nodeInformation, triangleSets, _combinedTriangleTreeStatistics);
    }

    private sealed record Node(Node? Parent, AxisAlignedBox BoundingBox, ISubDividableTriangleSet[] TexturedTriangleSets, List<Node> Children)
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