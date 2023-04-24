namespace Rays.Scenes;

public interface I2DSceneFactory
{
    I2DScene Create(IPolygonDrawer polygonDrawer);
}