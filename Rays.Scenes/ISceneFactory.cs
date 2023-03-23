namespace Rays.Scenes;

public interface ISceneFactory
{
    IScene Create(IPolygonDrawer polygonDrawer);
}
