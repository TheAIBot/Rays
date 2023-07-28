namespace Rays._3D;

public sealed record Node(AxisAlignedBox BoundingBox, ISubDividableTriangleSet[] TexturedTriangleSets, List<Node> Children)
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