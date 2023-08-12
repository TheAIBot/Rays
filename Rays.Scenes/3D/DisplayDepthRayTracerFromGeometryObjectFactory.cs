using Rays._3D;

namespace Rays.Scenes;

public sealed class DisplayDepthRayTracerFromGeometryObjectFactory : I3DSceneGeometryObjectFactory
{
    private readonly ISceneInformationFactory _sceneInformationFactory;
    private readonly ICameraFactory _cameraFactory;

    public DisplayDepthRayTracerFromGeometryObjectFactory(ISceneInformationFactory sceneInformationFactory, ICameraFactory cameraFactory)
    {
        _sceneInformationFactory = sceneInformationFactory;
        _cameraFactory = cameraFactory;
    }

    public I3DScene Create(ITriangleSetIntersector triangleSetIntersector, IPolygonDrawer polygonDrawer)
    {
        SceneInformation sceneInformation = _sceneInformationFactory.Create(triangleSetIntersector);
        Camera camera = _cameraFactory.Create(sceneInformation, polygonDrawer);
        return new DisplayDepthRayTracer(camera, sceneInformation, polygonDrawer, triangleSetIntersector);
    }
}
