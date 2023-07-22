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

        var nodesToCheck = new PriorityQueue<int, float>();
        nodesToCheck.Enqueue(0, 0);
        while (nodesToCheck.Count > 0)
        {
            Node node = _nodes[nodesToCheck.Dequeue()];
            if (node.ContainsTriangles)
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

                // A node either contain nodes or triangles, never both.
                continue;
            }


            var nodeChildren = node.Children.GetAsSpan(_nodes);
            for (int i = 0; i < nodeChildren.Length; i++)
            {
                Node child = nodeChildren[i];
                if (child.BoundingBox.Intersects(optimizedRayBoxIntersection))
                {
                    float distance = Vector4.DistanceSquared(rayTriangleOptimizedIntersection.Start, new Vector4(child.BoundingBox.Center, 0));
                    nodesToCheck.Enqueue(node.Children.Index + i, distance);
                }
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

    public IEnumerable<Triangle> GetTriangles()
    {
        return _nodeTexturedTriangles.SelectMany(x => x.SelectMany(y => y.GetTriangles()));
    }

    public readonly record struct Node(AxisAlignedBox BoundingBox, SpanRange Children, int TexturedTrianglesIndex)
    {
        public bool ContainsTriangles => TexturedTrianglesIndex != -1;
    }

    public readonly record struct SpanRange(int Index, int Length)
    {
        public Span<T> GetAsSpan<T>(T[] array)
        {
            return array.AsSpan(Index, Length);
        }
    }
}
