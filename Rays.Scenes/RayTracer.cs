using Rays._3D;
using System.Numerics;
using System.Threading.Tasks.Dataflow;

namespace Rays.Scenes;

internal sealed class Camera
{
    private readonly Vector3 _cameraPosition = new Vector3(0, 0, 0);
    private readonly Vector3 _cameraDirection = new Vector3(1, 0, 0);
    private readonly Vector3 _cameraUpDirection = new Vector3(0, 1, 0);
    private readonly float _viewportDistance = 3;
    private readonly Vector2 _viewportSize = new Vector2(5, 5);
}

internal sealed class RayTracer : IScene
{
    private readonly Vector3 _cameraPosition = new Vector3(0, 0, 0);
    private readonly Vector3 _cameraDirection = new Vector3(1, 0, 0);
    private readonly Vector3 _cameraUpDirection = new Vector3(0, 1, 0);
    private readonly float _viewportDistance = 3;
    private readonly Vector2 _viewportSize = new Vector2(5, 5);
    private readonly TriangleTree _triangleTree;
    private readonly Triangle _triangleColor = new Triangle(new Vector3(255, 0, 0), new Vector3(0, 255, 0), new Vector3(0, 0, 255));
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
        var parallel = new TransformBlock<(int x, int y), (int x, int y, Color color)>(position =>
        {
            Vector3 viewPortPixelPosition = bottomLeftViewPort + position.x * horizontalChange + position.y * verticalChange;
            Vector3 rayDirection = viewPortPixelPosition - _cameraPosition;
            Ray ray = new Ray(viewPortPixelPosition, Vector3.Normalize(rayDirection));

            if (_triangleTree.TryGetIntersection(ray, out (TriangleIntersection intersection, Color color) triangleIntersection))
            {
                return (position.x, position.y, triangleIntersection.color);
            }


            return (position.x, position.y, new Color(255, 255, 255, 255));
        }, new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
        });

        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                await parallel.SendAsync((x, y));
            }
        }

        parallel.Complete();

        await foreach (var result in parallel.ReceiveAllAsync())
        {
            await _polygonDrawer.SetFillColorAsync(result.color);
            await _polygonDrawer.DrawPixelAsync(result.x, result.y);
        }

        await _polygonDrawer.RenderAsync();
        await parallel.Completion;
    }
}