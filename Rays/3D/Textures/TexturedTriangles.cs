using System.Numerics;

namespace Rays._3D;

public sealed class TexturedTriangles : ITexturedTriangles
{
    private readonly Triangle[] _textureTriangles;
    private readonly Image<Rgba32> _texture;
    public Triangle[] Triangles { get; }

    public TexturedTriangles(Triangle[] triangles, Triangle[] textureTriangles, Image<Rgba32> texture)
    {
        Triangles = triangles;
        _textureTriangles = textureTriangles;
        _texture = texture;
    }

    public Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection)
    {
        var triangleTextureCoordinates = _textureTriangles[triangleIndex];
        Vector3 interpolatedTextureCoordinate = triangleIntersection.Interpolate(triangleTextureCoordinates.CornerA,
                                                                                 triangleTextureCoordinates.CornerB,
                                                                                 triangleTextureCoordinates.CornerC);
        int texturePositionX = (int)(interpolatedTextureCoordinate.X * _texture.Width);
        int texturePositionY = (int)(interpolatedTextureCoordinate.Y * _texture.Height);

        Rgba32 textureColor = _texture[texturePositionX, texturePositionY];
        return new Color(textureColor.R, textureColor.G, textureColor.B, textureColor.A);
    }

    public ITexturedTriangles SubCopy(Func<Triangle, bool> filter)
    {
        List<Triangle> subTriangles = new List<Triangle>();
        List<Triangle> subTextureTriangles = new List<Triangle>();
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
}