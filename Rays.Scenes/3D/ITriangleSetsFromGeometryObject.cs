using Rays._3D;

namespace Rays.Scenes;

public interface ITriangleSetsFromGeometryObject
{
    ISubDividableTriangleSet[] Load(string zippedGeometryFilePath);
}
