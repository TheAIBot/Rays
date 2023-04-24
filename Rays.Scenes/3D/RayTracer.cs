using Rays._3D;
using System.Threading.Tasks.Dataflow;

namespace Rays.Scenes;

internal sealed class RayTracer : I3DScene
{
    public Camera Camera { get; }
    private readonly TriangleTree _triangleTree;
    private readonly IPolygonDrawer _polygonDrawer;

    public RayTracer(Camera camera, IPolygonDrawer polygonDrawer, ITexturedTriangles[] texturedTriangles)
    {
        Camera = camera;
        _polygonDrawer = polygonDrawer;
        _triangleTree = TriangleTreeBuilder.Create(texturedTriangles);
    }

    public async Task RenderAsync()
    {
        await _polygonDrawer.ClearAsync();
        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        var parallel = new ActionBlock<Point>(position => RaySetPixelColor(rayTraceViewPort, position), new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
        });

        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                await parallel.SendAsync(new Point(x, y));
            }
        }

        parallel.Complete();
        await parallel.Completion;

        await _polygonDrawer.RenderAsync();
    }

    private Task RaySetPixelColor(RayTraceViewPort rayTraceViewPort, Point pixelPosition)
    {
        Ray ray = rayTraceViewPort.GetRayForPixel(pixelPosition);

        Color color = new Color(20, 20, 20, 20);
        if (_triangleTree.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
        {
            color = triangleIntersection.color;
        }

        return _polygonDrawer.DrawPixelAsync(pixelPosition.X, pixelPosition.Y, color);
    }
}