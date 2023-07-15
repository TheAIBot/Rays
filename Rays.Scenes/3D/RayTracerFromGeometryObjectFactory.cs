using Rays._3D;

namespace Rays.Scenes;

public sealed class RayTracerFromGeometryObjectFactory : I3DSceneGeometryObjectFactory
{
    private readonly CameraFactory _cameraFactory;

    public string DisplayName => "Default";

    public RayTracerFromGeometryObjectFactory(CameraFactory cameraFactory)
    {
        _cameraFactory = cameraFactory;
    }

    public I3DScene Create(ITriangleSetIntersector triangleSetIntersector, IPolygonDrawer polygonDrawer)
    {
        return new RayTracer(_cameraFactory.Create(polygonDrawer), polygonDrawer, triangleSetIntersector);
    }
}
