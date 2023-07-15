using Rays._3D;

namespace Rays.Scenes
{
    public interface ITriangleSetIntersectorFromGeometryObject
    {
        string IntersectionTypeName { get; }

        ITriangleSetIntersector Create(string zippedGeometryFilePath);
    }
}