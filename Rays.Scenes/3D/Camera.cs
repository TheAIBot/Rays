using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

public sealed class Camera
{
    /// <summary>
    /// Gets the position of the camera.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// Gets the direction of the camera. This should be a unit vector.
    /// </summary>
    public Vector3 Direction { get; set; }

    /// <summary>
    /// Gets the up direction of the camera. This should be a unit vector.
    /// </summary>
    public Vector3 UpDirection { get; set; }

    /// <summary>
    /// Gets the field of view of the camera, in degrees.
    /// </summary>
    public float FieldOfView { get; set; }

    /// <summary>
    /// Gets the aspect ratio of the camera.
    /// </summary>
    public float AspectRatio { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Camera"/> class.
    /// </summary>
    /// <param name="position">The position of the camera.</param>
    /// <param name="direction">The direction of the camera.</param>
    /// <param name="fieldOfView">The field of view of the camera, in degrees.</param>
    public Camera(Vector3 position, Vector3 direction, Vector3 upDirection, float fieldOfView, float aspectRatio)
    {
        Position = position;
        Direction = Vector3.Normalize(direction);
        UpDirection = upDirection;
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
    }

    internal RayTraceViewPortV2 GetRayTraceViewPort(Point screenSize)
    {
        Vector3 right = Vector3.Cross(Direction, UpDirection);
        Vector3 up = Vector3.Cross(right, Direction);
        float convertedFieldOfView = MathF.Tan(FieldOfView / 2 * MathF.PI / 180);
        return new RayTraceViewPortV2(screenSize, Position.ToZeroExtendedVector4(), Direction.ToZeroExtendedVector4(), right.ToZeroExtendedVector4(), up.ToZeroExtendedVector4(), AspectRatio, convertedFieldOfView);
    }
}