using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

internal sealed class RayTracer : I3DScene
{
    public Camera Camera { get; }
    public SceneInformation Information { get; }
    private readonly ITriangleSetIntersector _triangleSetIntersector;
    private readonly IPolygonDrawer _polygonDrawer;

    public RayTracer(Camera camera, SceneInformation sceneInformation, IPolygonDrawer polygonDrawer, ITriangleSetIntersector triangleSetIntersector)
    {
        Camera = camera;
        Information = sceneInformation;
        _polygonDrawer = polygonDrawer;
        _triangleSetIntersector = triangleSetIntersector;
    }

    public async Task RenderAsync(CancellationToken cancellationToken)
    {
        await _polygonDrawer.ClearAsync();
        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        Parallel.For(0, _polygonDrawer.Size.X * _polygonDrawer.Size.Y, x => RaySetPixelColor(rayTraceViewPort, x, _polygonDrawer.Size.X));

        await _polygonDrawer.RenderAsync();
    }

    private ValueTask RaySetPixelColor(RayTraceViewPort rayTraceViewPort, int pixelIndex, int screenWidth)
    {
        (int pixelY, int pixelX) = Math.DivRem(pixelIndex, screenWidth);
        var pixelPosition = new Vector2(pixelX, pixelY);
        Ray ray = rayTraceViewPort.GetRayForPixel(pixelPosition);

        var color = new Color(20, 20, 20, 20);
        if (_triangleSetIntersector.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
        {
            color = triangleIntersection.color;
        }

        return _polygonDrawer.DrawPixelAsync(pixelX, pixelY, color);
    }
}