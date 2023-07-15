using Microsoft.Extensions.DependencyInjection;

namespace Rays.Scenes;

public static class RayTracer3DServices
{
    public static IServiceCollection Add3DRayTracerSceneServices(this IServiceCollection services)
    {
        services.AddSingleton<CameraFactory>();
        services.AddSingleton<TriangleSetsFromGeometryObject>();
        services.AddSingleton<ITriangleSetIntersectorFromGeometryObject, TriangleListFromGeometryObject>();
        services.AddSingleton<ITriangleSetIntersectorFromGeometryObject, TriangleTreeFromGeometryObject>();
        services.AddSingleton<I3DSceneGeometryObjectFactory, RayTracerFromGeometryObjectFactory>();
        services.AddSingleton<I3DSceneGeometryObjectFactory, DisplayDepthRayTracerFromGeometryObjectFactory>();

        return services;
    }
}