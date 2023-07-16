using System.Numerics;

namespace Rays.Scenes;

public sealed class CameraFactory
{
    public Camera Create(SceneInformation sceneInformation, IPolygonDrawer polygonDrawer)
    {
        const float factorToClosestTriangle = 1.3f;
        Vector3 cameraPosition = sceneInformation.BoundingBox.MaxPosition * factorToClosestTriangle;
        Vector3 cameraDirection = Vector3.Normalize(sceneInformation.BoundingBox.Center - cameraPosition);

        return new Camera(cameraPosition, cameraDirection, new Vector3(0, 0, -1), 90.0f, (float)polygonDrawer.Size.X / polygonDrawer.Size.Y);
    }
}