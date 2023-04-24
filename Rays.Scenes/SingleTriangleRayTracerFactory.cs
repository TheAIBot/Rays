using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

public sealed class SingleTriangleRayTracerFactory : ISceneFactory
{
    public IScene Create(IPolygonDrawer polygonDrawer)
    {
        Triangle[] triangles = new Triangle[]
        {
            new Triangle(new Vector3(10, -1, -1), new Vector3(10, 3, -2), new Vector3(10, 2, 4))
        };
        Triangle color = new Triangle(new Vector3(255, 0, 0), new Vector3(0, 255, 0), new Vector3(0, 0, 255));
        var camera = new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), 90.0f, (float)polygonDrawer.Size.X / polygonDrawer.Size.Y);
        return new RayTracer(camera, polygonDrawer, new[] { new SimpleColoredTriangles(triangles, color) });
    }
}