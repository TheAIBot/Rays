using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

internal sealed class RayTraceViewPort
{
    private readonly Vector3 _cameraPosition;
    private readonly Vector3 _horizontalChange;
    private readonly Vector3 _verticalChange;
    private readonly Vector3 _bottomLeftViewPort;

    public RayTraceViewPort(Vector3 cameraPosition, Vector3 horizontalChange, Vector3 verticalChange, Vector3 bottomLeftViewPort)
    {
        _cameraPosition = cameraPosition;
        _horizontalChange = horizontalChange;
        _verticalChange = verticalChange;
        _bottomLeftViewPort = bottomLeftViewPort;
    }

    public Ray GetRayForPixel(Point pixelPosition)
    {
        Vector3 viewPortPixelPosition = _bottomLeftViewPort + pixelPosition.X * _horizontalChange + pixelPosition.Y * _verticalChange;
        Vector3 rayDirection = viewPortPixelPosition - _cameraPosition;
        return new Ray(viewPortPixelPosition, Vector3.Normalize(rayDirection));
    }
}
