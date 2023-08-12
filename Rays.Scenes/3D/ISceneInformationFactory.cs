using Rays._3D;

namespace Rays.Scenes;

public interface ISceneInformationFactory
{
    SceneInformation Create(ITriangleSetIntersector triangleSetIntersector);
}
