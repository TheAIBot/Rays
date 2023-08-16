using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

public sealed class RayTraceViewPort
{
    private readonly Vector2 _screenSize;
    private readonly Vector2 _screenSizeAspectRatioChanged;
    private readonly Vector4 _position;
    private readonly Vector4 _direction;
    private readonly Vector4 _right;
    private readonly Vector4 _up;
    private readonly Vector4 _convertedFieldOfView;

    public RayTraceViewPort(Point screenSize, Vector4 position, Vector4 direction, Vector4 right, Vector4 up, float aspectRatio, float convertedFieldOfView)
    {
        _screenSize = new Vector2(screenSize.X, screenSize.Y);
        _screenSizeAspectRatioChanged = _screenSize * new Vector2(aspectRatio, 1);
        _position = position;
        _direction = direction;
        _right = right;
        _up = up;
        _convertedFieldOfView = new Vector4(convertedFieldOfView);
    }

    public Ray GetRayForPixel(Vector2 pixelPosition)
    {
        // Normalize the pixel coordinates to be between -1 and 1
        Vector2 uv = (2.0f * pixelPosition - _screenSize) / _screenSizeAspectRatioChanged;

        // The ray direction is a point in the camera's field of view corresponding to the pixel
        Vector4 rayDirection = _direction + (uv.X * _right + uv.Y * _up) * _convertedFieldOfView;

        // The ray starts at the camera's position and goes in the direction we just calculated
        return new Ray(_position, rayDirection);
    }
}