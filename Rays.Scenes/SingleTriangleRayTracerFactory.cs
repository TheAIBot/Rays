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
        return new RayTracer(polygonDrawer, triangles);
    }
}