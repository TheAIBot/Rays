using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

internal sealed class DisplayDepthRayTracer : I3DScene
{
    public Camera Camera { get; }
    public SceneInformation Information { get; }
    private readonly ITriangleSetIntersector _triangleSetIntersector;
    private readonly IPolygonDrawer _polygonDrawer;
    private readonly float[] _depthMap;

    public DisplayDepthRayTracer(Camera camera, SceneInformation sceneInformation, IPolygonDrawer polygonDrawer, ITriangleSetIntersector triangleSetIntersector)
    {
        Camera = camera;
        Information = sceneInformation;
        _polygonDrawer = polygonDrawer;
        _triangleSetIntersector = triangleSetIntersector;
        _depthMap = new float[polygonDrawer.Size.X * polygonDrawer.Size.Y];
    }

    public async Task RenderAsync(CancellationToken cancellationToken)
    {
        await _polygonDrawer.ClearAsync();

        RayTraceViewPort rayTraceViewPort = Camera.GetRayTraceViewPort(_polygonDrawer.Size);
        Parallel.For(0, _polygonDrawer.Size.X * _polygonDrawer.Size.Y, x => RaySetPixelColor(rayTraceViewPort, x, _polygonDrawer.Size.X));

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < _depthMap.Length; i++)
        {
            if (float.IsNaN(_depthMap[i]))
            {
                continue;
            }

            min = MathF.Min(min, _depthMap[i]);
            max = MathF.Max(max, _depthMap[i]);
        }

        float difference = MathF.Abs(max - min);
        float scalingToByteRange = difference / byte.MaxValue;
        var backgroundColor = new Color(20, 20, 20, 20);
        for (int i = 0; i < _depthMap.Length; i++)
        {
            Color color = backgroundColor;
            if (!float.IsNaN(_depthMap[i]))
            {
                byte colorValue = (byte)Math.Clamp((_depthMap[i] - min) / scalingToByteRange, byte.MinValue, byte.MaxValue);
                color = new Color(colorValue, colorValue, colorValue, colorValue);
            }

            (int pixelY, int pixelX) = Math.DivRem(i, _polygonDrawer.Size.X);
            await _polygonDrawer.DrawPixelAsync(pixelX, pixelY, color);
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
            _depthMap[pixelIndex] = Vector3.Distance(triangleIntersection.intersection.GetIntersection(ray), ray.Start);
        }
        else
        {
            _depthMap[pixelIndex] = float.NaN;
        }

        return ValueTask.CompletedTask;
    }
}
