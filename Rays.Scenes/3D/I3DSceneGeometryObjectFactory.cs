using Rays._3D;

namespace Rays.Scenes;

public interface I3DSceneGeometryObjectFactory
{
    I3DScene Create(ITriangleSetIntersector triangleSetIntersector, IPolygonDrawer polygonDrawer);
}
