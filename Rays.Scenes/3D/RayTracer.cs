using Rays._3D;
using System.Numerics;
using System.Threading.Channels;

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

    public Task RenderAsync(CancellationToken cancellationToken)
    {
        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        Parallel.For(0, _polygonDrawer.Size.X * _polygonDrawer.Size.Y, x => RaySetPixelColor(rayTraceViewPort, x, _polygonDrawer.Size.X));
        return Task.CompletedTask;
    }

    private void RaySetPixelColor(RayTraceViewPort rayTraceViewPort, int pixelIndex, int screenWidth)
    {
        (int pixelY, int pixelX) = Math.DivRem(pixelIndex, screenWidth);
        var pixelPosition = new Vector2(pixelX, pixelY);
        Ray ray = rayTraceViewPort.GetRayForPixel(pixelPosition);

        var color = new Color(20, 20, 20, 20);
        if (_triangleSetIntersector.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
        {
            color = triangleIntersection.color;
        }

        _polygonDrawer.DrawPixel(pixelX, pixelY, color);
    }
}

internal sealed class RayTracerThreadDisplayer : I3DScene
{
    private static int _threadCount = 0;
    private static Color[] _possibleThreadColors =
    [
        new Color(255, 0, 0, 80),
        new Color(0, 255, 0, 80),
        new Color(0, 0, 255, 80),
        new Color(255, 255, 0, 80),
        new Color(0, 255, 255, 80),
        new Color(255, 0, 255, 80),
        new Color(255, 255, 255, 80),
        new Color(0, 0, 0, 80),
    ];

    public Camera Camera { get; }
    public SceneInformation Information { get; }
    private readonly ITriangleSetIntersector _triangleSetIntersector;
    private readonly IPolygonDrawer _polygonDrawer;
    private readonly ThreadLocal<Color> _threadColor = new ThreadLocal<Color>(() =>
    {
        return _possibleThreadColors[Interlocked.Increment(ref _threadCount) % (_possibleThreadColors.Length)];
    }, true);

    public RayTracerThreadDisplayer(Camera camera, 
                                    SceneInformation sceneInformation, 
                                    IPolygonDrawer polygonDrawer, 
                                    ITriangleSetIntersector triangleSetIntersector)
    {
        Camera = camera;
        Information = sceneInformation;
        _polygonDrawer = polygonDrawer;
        _triangleSetIntersector = triangleSetIntersector;

        _ = Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
            do
            {
                Console.WriteLine(_threadColor.Values.Count);
            } while (await timer.WaitForNextTickAsync());
        });
    }

    public async Task RenderAsync(CancellationToken cancellationToken)
    {
        const int renderChunkSize = 16;
        int chunksWide = (_polygonDrawer.Size.X + (renderChunkSize - 1)) / renderChunkSize;
        int chunksHigh = (_polygonDrawer.Size.Y + (renderChunkSize - 1)) / renderChunkSize;
        Point[] renderChunks = new Point[chunksWide * chunksHigh];
        int chunkIndex = 0;

        for (int chunkY = 0; chunkY < chunksHigh; chunkY++)
        {
            for (int chunkX = 0; chunkX < chunksWide; chunkX++)
            {
                renderChunks[chunkIndex++] = new Point(chunkX * renderChunkSize, chunkY * renderChunkSize);
            }
        }
        Channel<Point> renderWork = Channel.CreateUnbounded<Point>();
        foreach (Point chunk in renderChunks)
        {
            await renderWork.Writer.WriteAsync(chunk);
        }
        renderWork.Writer.Complete();

        await _polygonDrawer.ClearAsync();
        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        Parallel.For(0, renderChunks.Length, _ => RaySetPixelColor(rayTraceViewPort, renderWork.Reader, _polygonDrawer.Size.X));

        await _polygonDrawer.RenderAsync();
    }

    private void RaySetPixelColor(RayTraceViewPort rayTraceViewPort, ChannelReader<Point> renderWork, int screenWidth)
    {
        Color threadColor = _threadColor.Value;
        renderWork.TryRead(out Point chunkIndex);

        const int renderChunkSize = 16;
        int maxPixelX = Math.Min(chunkIndex.X + renderChunkSize, _polygonDrawer.Size.X);
        int maxPixelY = Math.Min(chunkIndex.Y + renderChunkSize, _polygonDrawer.Size.Y);
        for (int pixelY = chunkIndex.Y; pixelY < maxPixelY; pixelY++)
        {
            for (int pixelX = chunkIndex.X; pixelX < maxPixelX; pixelX++)
            {
                var pixelPosition = new Vector2(pixelX, pixelY);
                Ray ray = rayTraceViewPort.GetRayForPixel(pixelPosition);

                var color = new Color(20, 20, 20, 20);
                if (_triangleSetIntersector.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
                {
                    color = triangleIntersection.color;
                }

                color = Color.Blend(color, threadColor, threadColor.Alpha);
                color = color with { Alpha = byte.MaxValue };
                _polygonDrawer.DrawPixel(pixelX, pixelY, color);
            }
        }
    }
}