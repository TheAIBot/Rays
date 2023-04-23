using System.Numerics;

namespace Rays.Scenes;

internal sealed class Camera
{
    private readonly Vector3 _position = new Vector3(0, 0, 0);
    private readonly Vector3 _direction = new Vector3(1, 0, 0);
    private readonly Vector3 _upDirection = new Vector3(0, 1, 0);
    private readonly float _viewportDistance = 3;
    private readonly Vector2 _viewportSize = new Vector2(5, 5);

    public Camera(Vector3 position, Vector3 direction, Vector3 upDirection, float viewportDistance, Vector2 viewportSize)
    {
        _position = position;
        _direction = direction;
        _upDirection = upDirection;
        _viewportDistance = viewportDistance;
        _viewportSize = viewportSize;
    }

    public RayTraceViewPort GetRayTraceViewPort(Point viewPortPixelCount)
    {
        Vector3 horizontalChange = GetHorizontalChange(viewPortPixelCount.X);
        Vector3 verticalChange = GetVerticalChange(viewPortPixelCount.Y);
        Vector3 bottomLeftViewPort = GetBottomLeftOfViewPort(horizontalChange, verticalChange, viewPortPixelCount);
        return new RayTraceViewPort(_position, horizontalChange, verticalChange, bottomLeftViewPort);
    }

    private Vector3 GetVerticalChange(int verticalPixelCount)
    {
        return _upDirection * (_viewportSize.Y / verticalPixelCount);
    }

    private Vector3 GetHorizontalChange(int horizontalPixelCount)
    {
        Vector3 horizontalDirection = Vector3.Cross(_direction - _position, _upDirection - _position);
        return horizontalDirection * (_viewportSize.X / horizontalPixelCount);
    }

    private Vector3 GetBottomLeftOfViewPort(Vector3 horizontalChange, Vector3 verticalChange, Point viewportPixelCount)
    {
        return _position
               + (_direction * _viewportDistance)
               - (horizontalChange * (viewportPixelCount.X / 2))
               - (verticalChange * (viewportPixelCount.Y / 2));
    }
}
