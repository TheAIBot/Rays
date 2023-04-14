using Rays._3D;
using Rays.GeometryLoaders.Geometry;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
using Rectangle = Rays.Polygons.Rectangle;

namespace Rays.Scenes;

internal sealed class Camera
{
    private readonly Vector3 _cameraPosition = new Vector3(0, 0, 0);
    private readonly Vector3 _cameraDirection = new Vector3(1, 0, 0);
    private readonly Vector3 _cameraUpDirection = new Vector3(0, 1, 0);
    private readonly float _viewportDistance = 3;
    private readonly Vector2 _viewportSize = new Vector2(5, 5);
}

internal sealed class RayTracer : IScene
{
    private readonly Vector3 _cameraPosition = new Vector3(0, 0, 0);
    private readonly Vector3 _cameraDirection = new Vector3(1, 0, 0);
    private readonly Vector3 _cameraUpDirection = new Vector3(0, 1, 0);
    private readonly float _viewportDistance = 3;
    private readonly Vector2 _viewportSize = new Vector2(5, 5);
    private readonly ITexturedTriangles[] _texturedTriangles;
    private readonly Triangle _triangleColor = new Triangle(new Vector3(255, 0, 0), new Vector3(0, 255, 0), new Vector3(0, 0, 255));
    private readonly IPolygonDrawer _polygonDrawer;

    public RayTracer(IPolygonDrawer polygonDrawer, ITexturedTriangles[] texturedTriangles)
    {
        _polygonDrawer = polygonDrawer;
        _texturedTriangles = texturedTriangles;
    }

    public async Task RenderAsync()
    {
        await _polygonDrawer.ClearAsync();
        Vector3 verticalChange = _cameraUpDirection * (_viewportSize.Y / _polygonDrawer.Size.Y);
        Vector3 cameraHorizontalDirection = Vector3.Cross(_cameraDirection - _cameraPosition, _cameraUpDirection - _cameraPosition);
        Vector3 horizontalChange = cameraHorizontalDirection * (_viewportSize.X / _polygonDrawer.Size.X);
        Vector3 bottomLeftViewPort = _cameraPosition
                                    + (_cameraDirection * _viewportDistance)
                                    - (horizontalChange * (_polygonDrawer.Size.X / 2))
                                    - (verticalChange * (_polygonDrawer.Size.Y / 2));
        var parallel = new TransformBlock<(int x, int y), (int x, int y, Color color)>(position =>
        {
            Vector3 viewPortPixelPosition = bottomLeftViewPort + position.x * horizontalChange + position.y * verticalChange;
            Vector3 rayDirection = viewPortPixelPosition - _cameraPosition;
            Ray ray = new Ray(viewPortPixelPosition, Vector3.Normalize(rayDirection));

            foreach (var texturedTriangles in _texturedTriangles)
            {
                if (TryGetIntersectionWithTriangles(ray, texturedTriangles, out Color intersectionColor))
                {
                    return (position.x, position.y, intersectionColor);
                }
            }

            return (position.x, position.y, new Color(255, 255, 255, 255));
        }, new ExecutionDataflowBlockOptions()
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
        });

        for (int y = 0; y < _polygonDrawer.Size.Y; y++)
        {
            for (int x = 0; x < _polygonDrawer.Size.X; x++)
            {
                await parallel.SendAsync((x, y));
            }
        }

        parallel.Complete();

        await foreach (var result in parallel.ReceiveAllAsync())
        {
            await _polygonDrawer.SetFillColorAsync(result.color);
            await _polygonDrawer.DrawFillAsync(new Rectangle(new Vector2(result.x, result.y), new Vector2(1, 0), new Vector2(0, 1)));
        }

        await _polygonDrawer.RenderAsync();
        await parallel.Completion;
    }

    private bool TryGetIntersectionWithTriangles(Ray ray, ITexturedTriangles triangles, out Color intersectionColor)
    {
        for (int i = 0; i < triangles.Triangles.Length; i++)
        {
            if (triangles.Triangles[i].TryGetIntersection(ray, out TriangleIntersection intersection))
            {
                intersectionColor = triangles.GetTriangleIntersectionTextureColor(i, intersection);
                return true;
            }
        }

        intersectionColor = default;
        return false;
    }
}

internal interface ITexturedTriangles
{
    Triangle[] Triangles { get; }

    Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection);
}

internal sealed class SimpleColoredTriangles : ITexturedTriangles
{
    private readonly Triangle _triangleColor;
    public Triangle[] Triangles { get; }

    public SimpleColoredTriangles(Triangle[] triangles, Triangle triangleColor)
    {
        Triangles = triangles;
        _triangleColor = triangleColor;
    }

    public Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection)
    {
        Vector3 color = triangleIntersection.Interpolate(_triangleColor.CornerA, _triangleColor.CornerB, _triangleColor.CornerC);
        return new Color((int)color.X, (int)color.Y, (int)color.Z, 255);
    }
}

internal sealed class TexturedTriangles : ITexturedTriangles
{
    private readonly Triangle<TextureCoordinate>[] _textureTriangles;
    private readonly Image<Rgba32> _texture;
    public Triangle[] Triangles { get; }

    public TexturedTriangles(Triangle[] triangles, Triangle<TextureCoordinate>[] textureTriangles, Image<Rgba32> texture)
    {
        Triangles = triangles;
        _textureTriangles = textureTriangles;
        _texture = texture;
    }

    public Color GetTriangleIntersectionTextureColor(int triangleIndex, TriangleIntersection triangleIntersection)
    {
        var triangleTextureCoordinates = _textureTriangles[triangleIndex];
        Vector3 interpolatedTextureCoordinate = triangleIntersection.Interpolate(triangleTextureCoordinates.CornerA.Coordinate,
                                                                                 triangleTextureCoordinates.CornerB.Coordinate,
                                                                                 triangleTextureCoordinates.CornerC.Coordinate);
        int texturePositionX = (int)(interpolatedTextureCoordinate.X * _texture.Width);
        int texturePositionY = (int)(interpolatedTextureCoordinate.Y * _texture.Height);

        Rgba32 textureColor = _texture[texturePositionX, texturePositionY];
        return new Color(textureColor.R, textureColor.G, textureColor.B, textureColor.A);
    }
}