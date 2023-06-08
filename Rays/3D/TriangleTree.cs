using System.Numerics;
using static Rays._3D.AxisAlignedBox;
using static Rays._3D.Triangle;

namespace Rays._3D;

public sealed class TriangleTree : ITriangleSetIntersector
{
    private readonly Node[] _nodes;
    private readonly ITriangleSetIntersector[][] _nodeTexturedTriangles;

    internal TriangleTree(Node[] nodes, ITriangleSetIntersector[][] nodeTexturedTriangles)
    {
        _nodes = nodes;
        _nodeTexturedTriangles = nodeTexturedTriangles;
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        var rayTriangleOptimizedIntersection = new RayTriangleOptimizedIntersection(ray);
        return TryGetIntersection(rayTriangleOptimizedIntersection, out triangleIntersection);
    }

    public bool TryGetIntersection(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection, out (TriangleIntersection intersection, Color color) triangleIntersection)
    {
        var optimizedRayBoxIntersection = new RayAxisAlignBoxOptimizedIntersection(rayTriangleOptimizedIntersection);

        var nodesToCheck = new Stack<Node>();
        nodesToCheck.Push(_nodes[0]);
        while (nodesToCheck.Count > 0)
        {
            Node node = nodesToCheck.Pop();
            if (node.TexturedTrianglesIndex != -1)
            {
                if (TryGetIntersectionWithTriangles(rayTriangleOptimizedIntersection, _nodeTexturedTriangles[node.TexturedTrianglesIndex], out triangleIntersection))
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
                    nodeScores.Add(new NodeScore(i, Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, new Vector4(child.BoundingBox.Center, 0))));
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

    private bool TryGetIntersectionWithTriangles(RayTriangleOptimizedIntersection rayTriangleOptimizedIntersection, ITriangleSetIntersector[] texturedTriangleSets, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        foreach (var texturedTriangles in texturedTriangleSets)
        {
            if (!texturedTriangles.TryGetIntersection(rayTriangleOptimizedIntersection, out (TriangleIntersection intersection, Color color) triangleIntersection))
            {
                continue;
            }

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
