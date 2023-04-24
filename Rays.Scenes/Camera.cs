using System.Numerics;

namespace Rays.Scenes;

public sealed class Camera
{
    private readonly Vector3 _position;
    private readonly Vector3 _direction;
    private readonly Vector3 _upDirection;
    private readonly float _fieldOfViewRadians;
    private readonly float _aspectRatio;

    public Camera(Vector3 position, Vector3 direction, Vector3 upDirection, float fieldOfViewDegrees, float aspectRatio)
    {
        _position = position;
        _direction = direction;
        _upDirection = upDirection;
        _fieldOfViewRadians = fieldOfViewDegrees * (MathF.PI / 180.0f);
        _aspectRatio = aspectRatio;
    }

    internal RayTraceViewPort GetRayTraceViewPort(Point viewPortPixelCount)
    {
        Vector3 horizontalDirection = Vector3.Cross(_direction - _position, _upDirection - _position);
        return new RayTraceViewPort(_position,
                                    _direction,
                                    horizontalDirection,
                                    _upDirection,
                                    (_fieldOfViewRadians * _aspectRatio) / viewPortPixelCount.X,
                                    _fieldOfViewRadians / viewPortPixelCount.Y,
                                    viewPortPixelCount);
    }
}
