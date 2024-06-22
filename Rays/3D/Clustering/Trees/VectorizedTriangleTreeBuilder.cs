namespace Rays._3D;

public sealed class VectorizedTriangleTreeBuilder
{
    private readonly CombinedTriangleTreeStatistics _combinedTriangleTreeStatistics;

    public VectorizedTriangleTreeBuilder(CombinedTriangleTreeStatistics combinedTriangleTreeStatistics)
    {
        _combinedTriangleTreeStatistics = combinedTriangleTreeStatistics;
    }

    public VectorizedTriangleTree Create(Node root)
    {
        int nodeCount = root.CountNodes();
        var nodeBoundingBoxes = new AxisAlignedBox[nodeCount];
        var nodeInformation = new NodeInformation[nodeCount];
        var triangleSets = new ISubDividableTriangleSet[root.CountNodes(x => x.Children.Count == 0)][];
        int texturedSetsIndex = 0;
        int lastNodeIndex = nodeCount - 1;
        var nodeToIndex = new Dictionary<Node, int>();
        foreach (var node in root.ReverseBreadthFirstOrder())
        {
            if (node.Children.Count == 0)
            {
                nodeBoundingBoxes[lastNodeIndex] = AxisAlignedBox.GetBoundingBoxForTriangles(node.TexturedTriangleSets.SelectMany(x => x.GetTriangles()));
                nodeInformation[lastNodeIndex] = NodeInformation.CreateLeafNode(texturedSetsIndex);
                triangleSets[texturedSetsIndex] = node.TexturedTriangleSets;
                texturedSetsIndex++;
            }
            else
            {
                int firstChildIndex = nodeToIndex[node.Children[0]];
                nodeBoundingBoxes[lastNodeIndex] = AxisAlignedBox.GetBoundingBoxForBoxes(node.Children.Select(x => nodeToIndex[x]).Select(x => nodeBoundingBoxes[x]));
                nodeInformation[lastNodeIndex] = NodeInformation.CreateParentNode(firstChildIndex, node.Children.Count);
            }

            nodeToIndex.Add(node, lastNodeIndex);
            lastNodeIndex--;
        }

        return new VectorizedTriangleTree(nodeBoundingBoxes, nodeInformation, triangleSets, _combinedTriangleTreeStatistics);
    }
}