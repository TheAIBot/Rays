namespace Rays.Scenes;

public interface ICameraFactory
{
    Camera Create(SceneInformation sceneInformation, IPolygonDrawer polygonDrawer);
}
