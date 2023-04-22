﻿using Rays._3D;
using System.Numerics;
using System.Threading.Tasks.Dataflow;

namespace Rays.Scenes;

internal sealed class RayTracer : IScene
{
    private readonly Vector3 _cameraPosition = new Vector3(0, 0, 0);
    private readonly Vector3 _cameraDirection = new Vector3(1, 0, 0);
    private readonly Vector3 _cameraUpDirection = new Vector3(0, 1, 0);
    private readonly float _viewportDistance = 3;
    private readonly Vector2 _viewportSize = new Vector2(5, 5);
    private readonly TriangleTree _triangleTree;
    private readonly IPolygonDrawer _polygonDrawer;

    public RayTracer(IPolygonDrawer polygonDrawer, ITexturedTriangles[] texturedTriangles)
    {
        _polygonDrawer = polygonDrawer;
        _triangleTree = TriangleTreeBuilder.Create(texturedTriangles);
    }

    public async Task RenderAsync()
    {
        await _polygonDrawer.ClearAsync();
        Vector3 verticalChange = _cameraUpDirection * (_viewportSize.Y / _polygonDrawer.Size.Y);
        Vector3 cameraHorizontalDirection = Vector3.Cross(_cameraDirection - _cameraPosition, _cameraUpDirection - _cameraPosition);
        Vector3 horizontalChange = cameraHorizontalDirection * (_viewportSize.X / _polygonDrawer.Size.X);
        Vector3 bottomLeftViewPort = _cameraPosition
                                   + (_cameraDirection * _viewportDistance)
                                   - (horizontalChange * (_polygonDrawer.Size.X / 2))
                                   - (verticalChange * (_polygonDrawer.Size.Y / 2));
        var parallel = new ActionBlock<Point>(position => RaySetPixelColor(bottomLeftViewPort, horizontalChange, verticalChange, position), new ExecutionDataflowBlockOptions()
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

    private Task RaySetPixelColor(Vector3 bottomLeftViewPort, Vector3 horizontalChange, Vector3 verticalChange, Point position)
    {
        Vector3 viewPortPixelPosition = bottomLeftViewPort + position.X * horizontalChange + position.Y * verticalChange;
        Vector3 rayDirection = viewPortPixelPosition - _cameraPosition;
        var ray = new Ray(viewPortPixelPosition, Vector3.Normalize(rayDirection));

        Color color = default;
        if (_triangleTree.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
        {
            color = triangleIntersection.color;
        }

        return _polygonDrawer.DrawPixelAsync(position.X, position.Y, color);
    }
}