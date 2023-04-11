using Rays._3D;
using System.Numerics;
using Rectangle = Rays.Polygons.Rectangle;

namespace Rays.Scenes;

internal sealed class RayTracer : IScene
{
    private readonly Vector3 _cameraPosition = new Vector3(0, 0, 0);
    private readonly Vector3 _cameraDirection = new Vector3(1, 0, 0);
    private readonly Vector3 _cameraUpDirection = new Vector3(0, 1, 0);
    private readonly float _viewportDistance = 3;
    private readonly Vector2 _viewportSize = new Vector2(5, 5);
    private readonly Triangle[] _triangles;
    private readonly IPolygonDrawer _polygonDrawer;

    public RayTracer(IPolygonDrawer polygonDrawer, Triangle[] triangles)
    {
        _polygonDrawer = polygonDrawer;
        _triangles = triangles;
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
        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                Vector3 viewPortPixelPosition = bottomLeftViewPort + x * horizontalChange + y * verticalChange;
                Vector3 rayDirection = viewPortPixelPosition - _cameraPosition;
                Ray ray = new Ray(viewPortPixelPosition, Vector3.Normalize(rayDirection));

                for (int i = 0; i < _triangles.Length; i++)
                {
                    if (_triangles[i].TryGetIntersection(ray, out Vector3 _))
                    {
                        await _polygonDrawer.DrawFillAsync(new Rectangle(new Vector2(x, y), new Vector2(1, 0), new Vector2(0, 1)));
                        break;
                    }
                }
            }
        }

        await _polygonDrawer.RenderAsync();
    }
}