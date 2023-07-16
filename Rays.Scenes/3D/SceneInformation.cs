using Rays._3D;

namespace Rays.Scenes;

public sealed class SceneInformation
{
    public int TriangleCount { get; }
    public AxisAlignedBox BoundingBox { get; }

    public SceneInformation(int triangleCount, AxisAlignedBox boundingBox)
    {
        TriangleCount = triangleCount;
        BoundingBox = boundingBox;
    }
}