using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

internal sealed class RayTraceViewPort
{
    private readonly Vector3 _cameraPosition;
    private readonly Vector3 _cameraDirection;
    private readonly Vector3 _horizontalDirection;
    private readonly Vector3 _verticalDirection;
    private readonly float _horizontalChange;
    private readonly float _verticalChange;
    private readonly Point _screenSize;

    public RayTraceViewPort(Vector3 cameraPosition, Vector3 cameraDirection, Vector3 horizontalDirection, Vector3 verticalDirection, float horizontalChange, float verticalChange, Point screenSize)
    {
        _cameraPosition = cameraPosition;
        _cameraDirection = cameraDirection;
        _horizontalDirection = horizontalDirection;
        _verticalDirection = verticalDirection;
        _horizontalChange = horizontalChange;
        _verticalChange = verticalChange;
        _screenSize = screenSize;
    }

    public Ray GetRayForPixel(Point pixelPosition)
    {
        Matrix4x4 horizontalRotation = Matrix4x4.CreateFromAxisAngle(_horizontalDirection, _horizontalChange * (pixelPosition.X - (_screenSize.X / 2)));
        Matrix4x4 verticalRotation = Matrix4x4.CreateFromAxisAngle(_verticalDirection, _verticalChange * (pixelPosition.Y - (_screenSize.Y / 2)));
        Vector4 rayDirection = Vector4.Transform(_cameraDirection, horizontalRotation * verticalRotation);
        return new Ray(_cameraPosition, Vector3.Normalize(new Vector3(rayDirection.X, rayDirection.Y, rayDirection.Z)));
    }
}
