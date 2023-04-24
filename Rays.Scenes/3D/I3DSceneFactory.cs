namespace Rays.Scenes;

public interface I3DSceneFactory
{
    I3DScene Create(IPolygonDrawer polygonDrawer);
}
