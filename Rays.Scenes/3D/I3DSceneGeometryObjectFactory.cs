using Rays._3D;

namespace Rays.Scenes;

public interface I3DSceneGeometryObjectFactory
{
    string DisplayName { get; }
    I3DScene Create(ITriangleSetIntersector triangleSetIntersector, IPolygonDrawer polygonDrawer);
}
