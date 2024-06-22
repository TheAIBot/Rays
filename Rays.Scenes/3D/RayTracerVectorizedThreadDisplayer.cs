using Rays._3D;
using System.Numerics;
using System.Threading.Channels;

namespace Rays.Scenes;

internal sealed class RayTracerVectorizedThreadDisplayer : I3DScene
{
    private const int renderChunkSize = 32;

    public Camera Camera { get; }
    public SceneInformation Information { get; }
    private readonly ITriangleSetIntersector _triangleSetIntersector;
    private readonly IPolygonDrawer _polygonDrawer;

    public RayTracerVectorizedThreadDisplayer(Camera camera,
                                              SceneInformation sceneInformation,
                                              IPolygonDrawer polygonDrawer,
                                              ITriangleSetIntersector triangleSetIntersector)
    {
        Camera = camera;
        Information = sceneInformation;
        _polygonDrawer = polygonDrawer;
        _triangleSetIntersector = triangleSetIntersector;
    }

    public async Task RenderAsync(CancellationToken cancellationToken)
    {
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

        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        Parallel.For(0, renderChunks.Length, _ => RaySetPixelColor(rayTraceViewPort, renderWork.Reader, _polygonDrawer.Size.X));
    }

    private void RaySetPixelColor(RayTraceViewPort rayTraceViewPort, ChannelReader<Point> renderWork, int screenWidth)
    {
        renderWork.TryRead(out Point chunkIndex);

        const int subChunkXMaxSize = 4;
        const int subChunkYMaxSize = 4;
        Span<Ray> rays = stackalloc Ray[subChunkXMaxSize * subChunkYMaxSize];
        Span<bool> raysHit = stackalloc bool[subChunkXMaxSize * subChunkYMaxSize];
        Span<(TriangleIntersection intersection, Color color)> triangleIntersections = stackalloc (TriangleIntersection intersection, Color color)[subChunkXMaxSize * subChunkYMaxSize];

        int maxPixelX = Math.Min(chunkIndex.X + renderChunkSize, _polygonDrawer.Size.X);
        int maxPixelY = Math.Min(chunkIndex.Y + renderChunkSize, _polygonDrawer.Size.Y);

        for (int pixelY = chunkIndex.Y; pixelY < maxPixelY; pixelY += subChunkYMaxSize)
        {
            int subChunkMaxPixelY = Math.Min(pixelY + subChunkYMaxSize, _polygonDrawer.Size.Y);

            for (int pixelX = chunkIndex.X; pixelX < maxPixelX; pixelX += subChunkXMaxSize)
            {
                int subChunkMaxPixelX = Math.Min(pixelX + subChunkXMaxSize, _polygonDrawer.Size.X);
                int subChunkRayCount = (subChunkMaxPixelX - pixelX) * (subChunkMaxPixelY - pixelY);

                int rayIndex = 0;
                for (int subPixelY = pixelY; subPixelY < subChunkMaxPixelY; subPixelY++)
                {
                    for (int subPixelX = pixelX; subPixelX < subChunkMaxPixelX; subPixelX++)
                    {
                        var pixelPosition = new Vector2(subPixelX, subPixelY);
                        rays[rayIndex] = rayTraceViewPort.GetRayForPixel(pixelPosition);

                        rayIndex++;
                    }
                }

                _triangleSetIntersector.TryGetIntersections(rays, raysHit, triangleIntersections);

                rayIndex = 0;
                for (int subPixelY = pixelY; subPixelY < subChunkMaxPixelY; subPixelY++)
                {
                    for (int subPixelX = pixelX; subPixelX < subChunkMaxPixelX; subPixelX++)
                    {
                        var color = new Color(20, 20, 20, 20);
                        if (raysHit[rayIndex])
                        {
                            color = triangleIntersections[rayIndex].color;
                        }

                        _polygonDrawer.DrawPixel(subPixelX, subPixelY, color);

                        rayIndex++;
                    }
                }
            }
        }
    }
}