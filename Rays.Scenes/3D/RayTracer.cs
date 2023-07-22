using Rays._3D;

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
        await Parallel.ForEachAsync(GetPixelPositions(_polygonDrawer.Size), (position, _) => RaySetPixelColor(rayTraceViewPort, position));

        await _polygonDrawer.RenderAsync();
    }

    private IEnumerable<Point> GetPixelPositions(Point screenSize)
    {
        for (int y = 0; y < screenSize.Y; y++)
        {
            for (int x = 0; x < screenSize.X; x++)
            {
                yield return new Point(x, y);
            }
        }
    }

    private ValueTask RaySetPixelColor(RayTraceViewPort rayTraceViewPort, Point pixelPosition)
    {
        Ray ray = rayTraceViewPort.GetRayForPixel(pixelPosition);

        Color color = new Color(20, 20, 20, 20);
        if (_triangleSetIntersector.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
        {
            color = triangleIntersection.color;
        }

        return _polygonDrawer.DrawPixelAsync(pixelPosition.X, pixelPosition.Y, color);
    }
}