using System.Numerics;
using static Rays._3D.AxisAlignedBox;
using static Rays._3D.Triangle;

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
        var optimizedRayBoxIntersection = new RayAxisAlignBoxOptimizedIntersection(ray);

        var nodesToCheck = new Stack<Node>();
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

            var nodeScores = new List<NodeScore>();
            var nodeChildren = node.Children.GetAsSpan(_nodes);
            for (int i = 0; i < nodeChildren.Length; i++)
            {
                Node child = nodeChildren[i];
                if (child.BoundingBox.Intersects(optimizedRayBoxIntersection))
                {
                    nodeScores.Add(new NodeScore(i, Vector3.DistanceSquared(ray.Start, child.BoundingBox.Center)));
                }
            }

            foreach (var nodeScore in nodeScores.OrderBy(x => x.Distance))
            {
                nodesToCheck.Push(nodeChildren[nodeScore.Index]);
            }
        }

        triangleIntersection = default;
        return false;
    }

    private bool TryGetIntersectionWithTriangles(Ray ray, ITexturedTriangles[] texturedTriangleSets, out (TriangleIntersection intersection, Color color) intersection)
    {
        var rayTriangleOptimizedIntersection = new RayTriangleOptimizedIntersection(ray);

        intersection = default;
        float bestDistance = float.MaxValue;
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            for (int i = 0; i < texturedTriangles.Triangles.Length; i++)
            {
                if (!texturedTriangles.Triangles[i].TryGetIntersection(rayTriangleOptimizedIntersection, out TriangleIntersection triangleIntersection))
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

    private readonly record struct NodeScore(int Index, float Distance);

    public readonly record struct Node(AxisAlignedBox BoundingBox, SpanRange Children, int TexturedTrianglesIndex);

    public readonly record struct SpanRange(int Index, int Length)
    {
        public Span<T> GetAsSpan<T>(T[] array)
        {
            return array.AsSpan(Index, Length);
        }
    }
}
