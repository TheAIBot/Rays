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

        float bestDistance = float.MaxValue;
        triangleIntersection = default;

        var nodesToCheck = new Stack<Node>();
        nodesToCheck.Push(_nodes[0]);
        Span<NodeScore> maxNodeScores = stackalloc NodeScore[TriangleTreeBuilder.MaxChildCount];
        while (nodesToCheck.Count > 0)
        {
            Node node = nodesToCheck.Pop();
            if (node.TexturedTrianglesIndex != -1)
            {
                if (TryGetIntersectionWithTriangles(rayTriangleOptimizedIntersection, _nodeTexturedTriangles[node.TexturedTrianglesIndex], out var intersection))
                {
                    float distance = Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, intersection.intersection.GetIntersection(rayTriangleOptimizedIntersection));
                    if (distance > bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    triangleIntersection = intersection;
                }

                // A node either contains leaf nodes or triangles, never both.
                continue;
            }


            var nodeChildren = node.Children.GetAsSpan(_nodes);
            int nodeScoreCount = 0;
            for (int i = 0; i < nodeChildren.Length; i++)
            {
                Node child = nodeChildren[i];
                if (child.BoundingBox.Intersects(optimizedRayBoxIntersection))
                {
                    maxNodeScores[nodeScoreCount++] = new NodeScore(i, Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, new Vector4(child.BoundingBox.Center, 0)));
                }
            }

            if (nodeScoreCount == 0)
            {
                continue;
            }

            Span<NodeScore> nodeScores = maxNodeScores.Slice(0, nodeScoreCount);
            nodeScores.Sort();
            foreach (var nodeScore in nodeScores)
            {
                nodesToCheck.Push(nodeChildren[nodeScore.Index]);
            }
        }

        return bestDistance != float.MaxValue;
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

    private readonly record struct NodeScore(int Index, float Distance) : IComparable<NodeScore>
    {
        public int CompareTo(NodeScore other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }

    public readonly record struct Node(AxisAlignedBox BoundingBox, SpanRange Children, int TexturedTrianglesIndex);

    public readonly record struct SpanRange(int Index, int Length)
    {
        public Span<T> GetAsSpan<T>(T[] array)
        {
            return array.AsSpan(Index, Length);
        }
    }
}
