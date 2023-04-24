using System.Numerics;

namespace Rays.Scenes;

public sealed class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }
    public Vector3 UpDirection { get; set; }
    public float FieldOfViewRadians { get; set; }
    public float AspectRatio { get; set; }

    public Camera(Vector3 position, Vector3 direction, Vector3 upDirection, float fieldOfViewDegrees, float aspectRatio)
    {
        Position = position;
        Direction = direction;
        UpDirection = upDirection;
        FieldOfViewRadians = fieldOfViewDegrees * (MathF.PI / 180.0f);
        AspectRatio = aspectRatio;
    }

    internal RayTraceViewPort GetRayTraceViewPort(Point viewPortPixelCount)
    {
        Vector3 horizontalDirection = Vector3.Cross(Direction, UpDirection);

        return new RayTraceViewPort(Position,
                                    Direction,
                                    horizontalDirection,
                                    UpDirection,
                                    (FieldOfViewRadians * AspectRatio) / viewPortPixelCount.X,
                                    -FieldOfViewRadians / viewPortPixelCount.Y,
                                    viewPortPixelCount);
    }
}
