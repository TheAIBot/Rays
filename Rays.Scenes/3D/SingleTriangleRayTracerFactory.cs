using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

public sealed class SingleTriangleRayTracerFactory : I3DSceneFactory
{
    private readonly SceneInformationFactory _sceneInformationFactory;

    public SingleTriangleRayTracerFactory(SceneInformationFactory sceneInformationFactory)
    {
        _sceneInformationFactory = sceneInformationFactory;
    }

    public I3DScene Create(IPolygonDrawer polygonDrawer)
    {
        Triangle[] triangles = new Triangle[]
        {
            new Triangle(new Vector3(10, -1, -1), new Vector3(10, 3, -2), new Vector3(10, 2, 4))
        };
        Triangle color = new Triangle(new Vector3(255, 0, 0), new Vector3(0, 255, 0), new Vector3(0, 0, 255));
        ITriangleSetIntersector triangleIntersector = new TriangleList(new[] { new SimpleColoredTriangles(triangles, color) });

        var camera = new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 90.0f, (float)polygonDrawer.Size.X / polygonDrawer.Size.Y);
        SceneInformation sceneInformation = _sceneInformationFactory.Create(triangleIntersector);
        return new RayTracer(camera, sceneInformation, polygonDrawer, triangleIntersector);
    }
}