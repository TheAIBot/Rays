using Rays._3D;
using System.Numerics;

namespace Rays.Scenes;

public sealed class Camera
{
    /// <summary>
    /// Gets the position of the camera.
    /// </summary>
    public Vector4 Position { get; set; }

    /// <summary>
    /// Gets the direction of the camera. This should be a unit vector.
    /// </summary>
    public Vector4 Direction { get; set; }

    /// <summary>
    /// Gets the up direction of the camera. This should be a unit vector.
    /// </summary>
    public Vector4 UpDirection { get; set; }

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
    public Camera(Vector4 position, Vector4 direction, Vector4 upDirection, float fieldOfView, float aspectRatio)
    {
        Position = position;
        Direction = Vector4.Normalize(direction);
        UpDirection = upDirection;
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
    }

    internal RayTraceViewPort GetRayTraceViewPort(Point screenSize)
    {
        Vector4 right = Direction.Cross(UpDirection);
        Vector4 up = right.Cross(Direction);
        float convertedFieldOfView = MathF.Tan(FieldOfView / 2 * MathF.PI / 180);
        return new RayTraceViewPort(screenSize, Position, Direction, right, up, AspectRatio, convertedFieldOfView);
    }
}