using Rays._3D;

namespace Rays.Scenes;

internal sealed class RayTraceViewPort
{
    private readonly Camera _camera;
    private readonly Point _screenSize;

    public RayTraceViewPort(Camera camera, Point screenSize)
    {
        _camera = camera;
        _screenSize = screenSize;
    }

    public Ray GetRayForPixel(Point pixelPosition)
    {
        return _camera.GetRay(pixelPosition.X, pixelPosition.Y, _screenSize.X, _screenSize.Y);
    }
}
