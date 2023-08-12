using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

public sealed class SingleTriangleRayTracerFactory : I3DSceneFactory
{
    private readonly ISceneInformationFactory _sceneInformationFactory;

    public SingleTriangleRayTracerFactory(ISceneInformationFactory sceneInformationFactory)
    {
        _sceneInformationFactory = sceneInformationFactory;
    }

    public I3DScene Create(IPolygonDrawer polygonDrawer)
    {
        var triangles = new Triangle[]
        {
            new Triangle(new Vector3(10, -1, -1), new Vector3(10, 3, -2), new Vector3(10, 2, 4))
        };
        var color = new Triangle(new Vector3(255, 0, 0), new Vector3(0, 255, 0), new Vector3(0, 0, 255));
        var triangleIntersector = new TriangleList(new[] { new SimpleColoredTriangles(triangles, color) });

        var camera = new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 90.0f, (float)polygonDrawer.Size.X / polygonDrawer.Size.Y);
        SceneInformation sceneInformation = _sceneInformationFactory.Create(triangleIntersector);
        return new RayTracer(camera, sceneInformation, polygonDrawer, triangleIntersector);
    }
}