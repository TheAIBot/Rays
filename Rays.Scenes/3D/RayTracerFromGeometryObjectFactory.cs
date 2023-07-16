using Rays._3D;

namespace Rays.Scenes;

public sealed class RayTracerFromGeometryObjectFactory : I3DSceneGeometryObjectFactory
{
    private readonly SceneInformationFactory _sceneInformationFactory;
    private readonly CameraFactory _cameraFactory;

    public RayTracerFromGeometryObjectFactory(SceneInformationFactory sceneInformationFactory, CameraFactory cameraFactory)
    {
        _sceneInformationFactory = sceneInformationFactory;
        _cameraFactory = cameraFactory;
    }

    public I3DScene Create(ITriangleSetIntersector triangleSetIntersector, IPolygonDrawer polygonDrawer)
    {
        SceneInformation sceneInformation = _sceneInformationFactory.Create(triangleSetIntersector);
        Camera camera = _cameraFactory.Create(sceneInformation, polygonDrawer);
        return new RayTracer(camera, sceneInformation, polygonDrawer, triangleSetIntersector);
    }
}
