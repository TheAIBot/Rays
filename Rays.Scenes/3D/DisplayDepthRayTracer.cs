using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

internal sealed class DisplayDepthRayTracer : I3DScene
{
    public Camera Camera { get; }
    public SceneInformation Information { get; }
    private readonly ITriangleSetIntersector _triangleSetIntersector;
    private readonly IPolygonDrawer _polygonDrawer;
    private readonly float[,] _depthMap;

    public DisplayDepthRayTracer(Camera camera, SceneInformation sceneInformation, IPolygonDrawer polygonDrawer, ITriangleSetIntersector triangleSetIntersector)
    {
        Camera = camera;
        Information = sceneInformation;
        _polygonDrawer = polygonDrawer;
        _triangleSetIntersector = triangleSetIntersector;
        _depthMap = new float[polygonDrawer.Size.X, polygonDrawer.Size.Y];
    }

    public async Task RenderAsync(CancellationToken cancellationToken)
    {
        await _polygonDrawer.ClearAsync();

        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        Parallel.For(0, _polygonDrawer.Size.X * _polygonDrawer.Size.Y, x => RaySetPixelColor(rayTraceViewPort, x, _polygonDrawer.Size.X));

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                if (float.IsNaN(_depthMap[x, y]))
                {
                    continue;
                }

                min = MathF.Min(min, _depthMap[x, y]);
                max = MathF.Max(max, _depthMap[x, y]);
            }
        }

        float difference = MathF.Abs(max - min);
        float scalingToByteRange = difference / byte.MaxValue;
        var backgroundColor = new Color(20, 20, 20, 20);
        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                Color color = backgroundColor;
                if (!float.IsNaN(_depthMap[x, y]))
                {
                    byte colorValue = (byte)Math.Clamp((_depthMap[x, y] - min) / scalingToByteRange, byte.MinValue, byte.MaxValue);
                    color = new Color(colorValue, colorValue, colorValue, colorValue);
                }

                await _polygonDrawer.DrawPixelAsync(x, y, color);
            }
        }

        await _polygonDrawer.RenderAsync();
    }

    private ValueTask RaySetPixelColor(RayTraceViewPort rayTraceViewPort, int pixelIndex, int screenWidth)
    {
        (int pixelY, int pixelX) = Math.DivRem(pixelIndex, screenWidth);
        var pixelPosition = new Point(pixelX, pixelY);
        Ray ray = rayTraceViewPort.GetRayForPixel(pixelPosition);

        if (_triangleSetIntersector.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
        {
            _depthMap[pixelPosition.X, pixelPosition.Y] = Vector3.Distance(triangleIntersection.intersection.GetIntersection(ray), ray.Start);
        }
        else
        {
            _depthMap[pixelPosition.X, pixelPosition.Y] = float.NaN;
        }

        return ValueTask.CompletedTask;
    }
}
