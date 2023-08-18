using System.Numerics;

namespace Rays.Scenes;

public sealed class CameraFactory : ICameraFactory
{
    public Camera Create(SceneInformation sceneInformation, IPolygonDrawer polygonDrawer)
    {
        const float factorToClosestTriangle = 1.3f;
        Vector4 cameraPosition = sceneInformation.BoundingBox.MaxPosition * factorToClosestTriangle;
        Vector4 cameraDirection = Vector4.Normalize(sceneInformation.BoundingBox.Center - cameraPosition);

        return new Camera(cameraPosition, cameraDirection, new Vector4(0, 0, -1, 0), 90.0f, (float)polygonDrawer.Size.X / polygonDrawer.Size.Y);
    }
}