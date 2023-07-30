namespace Rays._3D;

public interface INodeClusterBuilder
{
    Node Create(ISubDividableTriangleSet[] texturedTriangleSets);
}
