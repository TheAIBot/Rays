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
        Direction = direction;
        UpDirection = upDirection;
        FieldOfView = fieldOfView;
        AspectRatio = aspectRatio;
    }

    /// <summary>
    /// Returns a ray that goes from the camera's position in the direction corresponding to a given pixel.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <param name="screenWidth">The width of the screen, in pixels.</param>
    /// <param name="screenHeight">The height of the screen, in pixels.</param>
    /// <returns>A ray that goes from the camera's position in the direction corresponding to the pixel.</returns>
    public Ray GetRay(int x, int y, int screenWidth, int screenHeight)
    {
        // Normalize the pixel coordinates to be between -1 and 1
        float u = (2.0f * x - screenWidth) / (screenWidth * AspectRatio);
        float v = (2.0f * y - screenHeight) / screenHeight;

        // The camera's "right" vector is cross product of its direction and the up vector
        Vector3 right = Vector3.Cross(Direction, UpDirection);

        // The camera's "up" vector is then the cross product of the right vector and its direction
        Vector3 up = Vector3.Cross(right, Direction);

        // The ray direction is a point in the camera's field of view corresponding to the pixel
        Vector3 rayDirection = Direction + (u * right + v * up) * MathF.Tan(FieldOfView / 2 * MathF.PI / 180);

        // The ray starts at the camera's position and goes in the direction we just calculated
        return new Ray(Position, Vector3.Normalize(rayDirection));
    }

    internal RayTraceViewPort GetRayTraceViewPort(Point screenSize)
    {
        return new RayTraceViewPort(this, screenSize);
    }
}