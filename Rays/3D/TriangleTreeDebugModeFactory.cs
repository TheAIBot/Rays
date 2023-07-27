using System.Numerics;
using static Rays._3D.TriangleTree;

namespace Rays._3D;

public sealed class TriangleTreeDebugModeFactory
{
    private readonly TriangleTreeBuilder _triangleTreeBuilder;

    public TriangleTreeDebugModeFactory(TriangleTreeBuilder triangleTreeBuilder)
    {
        _triangleTreeBuilder = triangleTreeBuilder;
    }

    public TriangleTreeDebugMode Create(TriangleTree triangleTree)
    {
        TriangleTree[] trees = GetLayerBoundingBoxes(triangleTree.Nodes.ToArray()).Select(GetTrianglesForLayerBoundingBoxes)
                                                                                  .Select(_triangleTreeBuilder.Create)
                                                                                  .ToArray();
        return new TriangleTreeDebugMode(trees);
    }

    private static IEnumerable<LayerBoundingBoxes> GetLayerBoundingBoxes((AxisAlignedBox Box, NodeInformation NodeInformation)[] nodes)
    {
        Queue<((AxisAlignedBox Box, NodeInformation NodeInformation), int NodeLayer)> nodesToCheck = new Queue<((AxisAlignedBox Box, NodeInformation NodeInformation), int Layer)>();
        nodesToCheck.Enqueue((nodes[0], 1));
        int currentLayer = 1;

        List<AxisAlignedBox> boxesInLayer = new List<AxisAlignedBox>();
        while (nodesToCheck.Count > 0)
        {
            ((AxisAlignedBox box, NodeInformation nodeInformation), int nodeLayer) = nodesToCheck.Dequeue();
            if (nodeLayer != currentLayer)
            {

                yield return new LayerBoundingBoxes(currentLayer, boxesInLayer.ToArray());
                boxesInLayer.Clear();
                currentLayer = nodeLayer;
            }

            boxesInLayer.Add(box);

            if (nodeInformation.IsLeafNode)
            {
                continue;
            }

            int nodeChildStartIndex = nodeInformation.ChildStartIndex;
            int nodeChildCount = nodeInformation.ChildCount;
            for (int i = 0; i < nodeChildCount; i++)
            {
                int childIndex = nodeChildStartIndex + i;
                nodesToCheck.Enqueue((nodes[childIndex], nodeLayer + 1));
            }
        }

        yield return new LayerBoundingBoxes(currentLayer, boxesInLayer.ToArray());
    }

    private static SingleColoredTriangles[] GetTrianglesForLayerBoundingBoxes(LayerBoundingBoxes layerBoundingBoxes)
    {
        SingleColoredTriangles[][] boundingBoxTriangles = layerBoundingBoxes.BoundingBoxes.Select(CreateTrianglesForBoundingBox).ToArray();
        List<Triangle>[] combinedTriangles = new List<Triangle>[6];
        for (int i = 0; i < combinedTriangles.Length; i++)
        {
            combinedTriangles[i] = new List<Triangle>();
        }

        foreach (var boxTriangles in boundingBoxTriangles)
        {
            for (int i = 0; i < boxTriangles.Length; i++)
            {
                combinedTriangles[i].AddRange(boxTriangles[i].Triangles);
            }
        }

        SingleColoredTriangles[] combinedSingleColoredTriangles = new SingleColoredTriangles[combinedTriangles.Length];
        for (int i = 0; i < combinedTriangles.Length; i++)
        {
            combinedSingleColoredTriangles[i] = new SingleColoredTriangles(combinedTriangles[i].ToArray(), boundingBoxTriangles[0][i].TriangleColor);
        }

        return combinedSingleColoredTriangles;
    }

    private static SingleColoredTriangles[] CreateTrianglesForBoundingBox(AxisAlignedBox box)
    {
        static SingleColoredTriangles CreateSideTriangles(Vector3 color, AxisAlignedBox box, Vector4 startOffset, Vector4 side1, Vector4 side2, bool reversedTriangles)
        {
            Vector3 scaledColor = color * new Vector3(byte.MaxValue);
            Color rgba = new Color((byte)scaledColor.X, (byte)scaledColor.Y, (byte)scaledColor.Z, (byte)255);

            Vector3 start = (box.MinPosition + (box.Size * startOffset)).ToTruncatedVector3();
            Vector3 side1Change = (box.Size * side1).ToTruncatedVector3();
            Vector3 side2Change = (box.Size * side2).ToTruncatedVector3();
            Vector3 pointA = start;
            Vector3 pointB = start + side1Change;
            Vector3 pointC = start + side2Change;
            Vector3 pointD = start + side1Change + side2Change;
            if (!reversedTriangles)
            {
                return new SingleColoredTriangles(new Triangle[]
{
                new Triangle(pointB, pointA, pointC),
                new Triangle(pointD, pointB, pointC)
}, rgba);
            }
            else
            {
                return new SingleColoredTriangles(new Triangle[]
{
                new Triangle(pointB, pointC, pointA),
                new Triangle(pointD, pointC, pointB)
}, rgba);
            }

        }

        var boundingBoxTriangles = new SingleColoredTriangles[6];
        boundingBoxTriangles[0] = CreateSideTriangles(new Vector3(0, 1, 1), box, new Vector4(0, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 1, 0, 0), false);
        boundingBoxTriangles[1] = CreateSideTriangles(new Vector3(1, 0, 1), box, new Vector4(0, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(1, 0, 0, 0), true);
        boundingBoxTriangles[2] = CreateSideTriangles(new Vector3(1, 1, 0), box, new Vector4(0, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(1, 0, 0, 0), false);
        boundingBoxTriangles[3] = CreateSideTriangles(new Vector3(1, 0, 0), box, new Vector4(1, 0, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 1, 0, 0), false);
        boundingBoxTriangles[4] = CreateSideTriangles(new Vector3(0, 1, 0), box, new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(1, 0, 0, 0), true);
        boundingBoxTriangles[5] = CreateSideTriangles(new Vector3(0, 0, 1), box, new Vector4(0, 0, 1, 0), new Vector4(0, 1, 0, 0), new Vector4(1, 0, 0, 0), false);
        return boundingBoxTriangles;
    }

    private sealed record LayerBoundingBoxes(int Layer, AxisAlignedBox[] BoundingBoxes);
}
