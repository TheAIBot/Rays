using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

public sealed class RayTraceViewPortV2
{
    private readonly Point _screenSize;
    private readonly Vector3 _position;
    private readonly Vector4 _direction;
    private readonly Vector4 _right;
    private readonly Vector4 _up;
    private readonly float _aspectRatio;
    private readonly float _convertedFieldOfView;

    public RayTraceViewPortV2(Point screenSize, Vector3 position, Vector4 direction, Vector4 right, Vector4 up, float aspectRatio, float convertedFieldOfView)
    {
        _screenSize = screenSize;
        _position = position;
        _direction = direction;
        _right = right;
        _up = up;
        _aspectRatio = aspectRatio;
        _convertedFieldOfView = convertedFieldOfView;
    }

    public Ray GetRayForPixel(Point pixelPosition)
    {
        // Normalize the pixel coordinates to be between -1 and 1
        float u = (2.0f * pixelPosition.X - _screenSize.X) / (_screenSize.X * _aspectRatio);
        float v = (2.0f * pixelPosition.Y - _screenSize.Y) / _screenSize.Y;

        // The ray direction is a point in the camera's field of view corresponding to the pixel
        Vector4 rayDirection = _direction + (u * _right + v * _up) * _convertedFieldOfView;

        // The ray starts at the camera's position and goes in the direction we just calculated
        return new Ray(_position, rayDirection.ToTruncatedVector3());
    }
}