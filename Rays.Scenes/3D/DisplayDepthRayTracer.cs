using Rays._3D;
using System.Numerics;
using System.Threading.Tasks.Dataflow;

namespace Rays.Scenes;

internal sealed class DisplayDepthRayTracer : I3DScene
{
    public Camera Camera { get; }
    private readonly ITriangleSetIntersector _triangleSetIntersector;
    private readonly IPolygonDrawer _polygonDrawer;
    private float[,] _depthMap;

    public DisplayDepthRayTracer(Camera camera, IPolygonDrawer polygonDrawer, ITriangleSetIntersector triangleSetIntersector)
    {
        Camera = camera;
        _polygonDrawer = polygonDrawer;
        _triangleSetIntersector = triangleSetIntersector;
        _depthMap = new float[polygonDrawer.Size.X, polygonDrawer.Size.Y];
    }

    public async Task RenderAsync(CancellationToken cancellationToken)
    {
        await _polygonDrawer.ClearAsync();
        Array.Clear(_depthMap);

        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        var parallel = new ActionBlock<Point>(position => RaySetPixelColor(rayTraceViewPort, position), new ExecutionDataflowBlockOptions()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
        });

        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                await parallel.SendAsync(new Point(x, y), cancellationToken);
            }
        }

        parallel.Complete();
        await parallel.Completion;

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                min = MathF.Min(min, _depthMap[x, y]);
                max = MathF.Max(max, _depthMap[x, y]);
            }
        }

        float difference = MathF.Abs(max - min);
        float scalingToByteRange = difference / byte.MaxValue;
        Color backgroundColor = new Color(20, 20, 20, 20);
        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                Color color = backgroundColor;
                if (!float.IsNaN(_depthMap[x, y]))
                {
                    byte colorValue = (byte)((_depthMap[x, y] + min) / scalingToByteRange);
                    color = new Color(colorValue, colorValue, colorValue, colorValue);
                }

                await _polygonDrawer.DrawPixelAsync(x, y, color);
            }
        }

        await _polygonDrawer.RenderAsync();
    }

    private Task RaySetPixelColor(RayTraceViewPort rayTraceViewPort, Point pixelPosition)
    {
        Ray ray = rayTraceViewPort.GetRayForPixel(pixelPosition);

        if (_triangleSetIntersector.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
        {
            if (triangleIntersection.intersection == default)
            {
                _depthMap[pixelPosition.X, pixelPosition.Y] = float.NaN;
                return Task.CompletedTask;
            }

            _depthMap[pixelPosition.X, pixelPosition.Y] = Vector3.Distance(triangleIntersection.intersection.GetIntersection(ray), ray.Start);
        }

        return Task.CompletedTask;
    }
}
