using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace Rays._3D;

public sealed class TexturedTriangles : ISubDividableTriangleSet
{
    private readonly Triangle[] _textureTriangles;
    private readonly Image<Rgba32> _texture;
    private readonly AxisAlignedBox _boundingBox;
    public Triangle[] Triangles { get; }

    public TexturedTriangles(Triangle[] triangles, Triangle[] textureTriangles, Image<Rgba32> texture)
    {
        Triangles = triangles;
        _textureTriangles = textureTriangles;
        _texture = texture;
        _boundingBox = AxisAlignedBox.GetBoundingBoxForTriangles(textureTriangles);
    }

    public AxisAlignedBox GetBoundingBox() => _boundingBox;
    public void OptimizeIntersectionFromSceneInformation(Vector4 cameraPosition, Frustum frustum) { }

    public void TryGetIntersections(ReadOnlySpan<Ray> rays, Span<bool> raysHit, Span<(TriangleIntersection intersection, Color color)> triangleIntersections)
    {
        for (int i = 0; i < rays.Length; i++)
        {
            raysHit[i] = TryGetIntersection(rays[i], out triangleIntersections[i]);
        }
    }

    public bool TryGetIntersection(Ray ray, out (TriangleIntersection intersection, Color color) intersection)
    {
        intersection = default;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < Triangles.Length; i++)
        {
            if (!Triangles[i].TryGetIntersection(ray, out TriangleIntersection triangleIntersection))
            {
                continue;
            }

            float distance = Vector4.DistanceSquared(ray.Start, triangleIntersection.GetIntersection(ray));
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            intersection.intersection = triangleIntersection;
            intersection.color = GetTriangleIntersectionTextureColor(i, triangleIntersection);
        }

        return bestDistance != float.MaxValue;
    }

    private Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection)
    {
        var triangleTextureCoordinates = _textureTriangles[triangleIndex];
        // Yeah so i don't know why but the textures are only correct if the texture points are provided in this weird order
        Vector4 interpolatedTextureCoordinate = triangleIntersection.Interpolate(triangleTextureCoordinates.CornerB,
                                                                                 triangleTextureCoordinates.CornerC,
                                                                                 triangleTextureCoordinates.CornerA);
        int texturePositionX = (int)(interpolatedTextureCoordinate.X * _texture.Width);
        // I have no idea why i need to flip the y axis but the textures are otherwise inverted
        int texturePositionY = (int)((1.0f - interpolatedTextureCoordinate.Y) * _texture.Height);

        // Not sure which strategy to use. Can either use wrapping or clamping logic.
        // Went for clamping since it is simpler to implement for now.
        texturePositionX = Math.Clamp(texturePositionX, 0, _texture.Width - 1);
        texturePositionY = Math.Clamp(texturePositionY, 0, _texture.Height - 1);

        Rgba32 textureColor = _texture[texturePositionX, texturePositionY];
        return new Color(textureColor.R, textureColor.G, textureColor.B, textureColor.A);
    }

    public ISubDividableTriangleSet SubCopy(Func<Triangle, bool> filter)
    {
        var subTriangles = new List<Triangle>();
        var subTextureTriangles = new List<Triangle>();
        for (int i = 0; i < Triangles.Length; i++)
        {
            if (filter(Triangles[i]))
            {
                subTriangles.Add(Triangles[i]);
                subTextureTriangles.Add(_textureTriangles[i]);
            }
        }

        return new TexturedTriangles(subTriangles.ToArray(), subTextureTriangles.ToArray(), _texture);
    }

    public ISubDividableTriangleSet SubCopy(IEnumerable<int> triangleIndexes)
    {
        var subTriangles = new List<Triangle>();
        var subTextureTriangles = new List<Triangle>();
        foreach (var triangleIndex in triangleIndexes)
        {
            subTriangles.Add(Triangles[triangleIndex]);
            subTextureTriangles.Add(_textureTriangles[triangleIndex]);
        }

        return new TexturedTriangles(subTriangles.ToArray(), subTextureTriangles.ToArray(), _texture);
    }

    public IEnumerable<Triangle> GetTriangles()
    {
        return Triangles;
    }
}