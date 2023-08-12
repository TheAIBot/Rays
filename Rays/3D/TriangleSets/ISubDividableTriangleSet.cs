namespace Rays._3D;

public interface ISubDividableTriangleSet : ITriangleSet, ITriangleSetIntersector
{
    ISubDividableTriangleSet SubCopy(Func<Triangle, bool> filter);

    ISubDividableTriangleSet SubCopy(IEnumerable<int> triangleIndexes);
}
