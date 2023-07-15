using Rays._3D;

namespace Rays.Scenes
{
    public interface ITriangleSetIntersectorFromGeometryObject
    {
        ITriangleSetIntersector Create(string zippedGeometryFilePath);
    }
}