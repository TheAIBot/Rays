using System.Numerics;

namespace Rays._3D;

public sealed class TriangleTree
{
    private readonly Node[] _nodes;
    private readonly ITexturedTriangles[][] _nodeTexturedTriangles;

    internal TriangleTree(Node[] nodes, ITexturedTriangles[][] nodeTexturedTriangles)
    {
        _nodes = nodes;
        _nodeTexturedTriangles = nodeTexturedTriangles;
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        Stack<Node> nodesToCheck = new Stack<Node>();
        nodesToCheck.Push(_nodes[0]);
        while (nodesToCheck.Count > 0)
        {
            Node node = nodesToCheck.Pop();
            if (node.TexturedTrianglesIndex != -1)
            {
                if (TryGetIntersectionWithTriangles(ray, _nodeTexturedTriangles[node.TexturedTrianglesIndex], out triangleIntersection))
                {
                    return true;
                }

                continue;
            }

            List<NodeScore> nodeScores = new List<NodeScore>();
            for (int i = 0; i < node.Children.Length; i++)
            {
                Node child = node.Children.Span[i];
                if (child.BoundingBox.Intersects(ray))
                {
                    nodeScores.Add(new NodeScore(i, Vector3.DistanceSquared(ray.Start, child.BoundingBox.Center)));
                }
            }

            foreach (var nodeScore in nodeScores.OrderBy(x => x.distance))
            {
                nodesToCheck.Push(node.Children.Span[nodeScore.index]);
            }
        }

        triangleIntersection = default;
        return false;
    }

    private bool TryGetIntersectionWithTriangles(Ray ray, ITexturedTriangles[] texturedTriangleSets, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            for (int i = 0; i < texturedTriangles.Triangles.Length; i++)
            {
                if (!texturedTriangles.Triangles[i].TryGetIntersection(ray, out TriangleIntersection triangleIntersection))
                {
                    continue;
                }

                float distance = Vector3.DistanceSquared(ray.Start, triangleIntersection.GetIntersection(ray));
                if (distance > bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                intersection.intersection = triangleIntersection;
                intersection.color = texturedTriangles.GetTriangleIntersectionTextureColor(i, triangleIntersection);
            }
        }

        return bestDistance != float.MaxValue;
    }

    private readonly record struct NodeScore(int index, float distance);

    public sealed record Node(AxisAlignedBox BoundingBox, Memory<Node> Children, int TexturedTrianglesIndex);
}
