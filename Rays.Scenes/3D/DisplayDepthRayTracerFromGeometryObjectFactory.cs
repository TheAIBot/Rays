using Rays._3D;

namespace Rays.Scenes;

public sealed class DisplayDepthRayTracerFromGeometryObjectFactory : I3DSceneGeometryObjectFactory
{
    private readonly CameraFactory _cameraFactory;

    public string DisplayName => "Depth";

    public DisplayDepthRayTracerFromGeometryObjectFactory(CameraFactory cameraFactory)
    {
        _cameraFactory = cameraFactory;
    }

    public I3DScene Create(ITriangleSetIntersector triangleSetIntersector, IPolygonDrawer polygonDrawer)
    {
        return new DisplayDepthRayTracer(_cameraFactory.Create(polygonDrawer), polygonDrawer, triangleSetIntersector);
    }
}
