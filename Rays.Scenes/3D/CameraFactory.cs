using System.Numerics;

namespace Rays.Scenes;

public sealed class CameraFactory
{
    public Camera Create(IPolygonDrawer polygonDrawer)
    {
        return new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 0, -1), 90.0f, (float)polygonDrawer.Size.X / polygonDrawer.Size.Y);
    }
}
