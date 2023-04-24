using Rays.Polygons;
using System.Numerics;
using Rectangle = Rays.Polygons.Rectangle;

namespace Rays.Scenes;

internal sealed class SpinningRectangle : I2DScene
{
    private readonly IPolygonDrawer _polygonDrawer;
    private readonly List<Wall> _staticWalls;
    private Rectangle _rectangle;

    public SpinningRectangle(IPolygonDrawer polygonDrawer, List<Wall> staticWalls, Rectangle rectangle)
    {
        _polygonDrawer = polygonDrawer;
        _staticWalls = staticWalls;
        _rectangle = rectangle;
    }

    public async Task RenderAsync()
    {
        List<Wall> walls = new List<Wall>(_staticWalls);
        walls.AddRange(_rectangle.GetAsWalls());

        var ray = new Ray(new Vector2(40, 50), Vector2.Normalize(new Vector2(-1, -1.5f)));
        List<Line> lines = Ray.GetRayPath(ray, 9, walls);

        await _polygonDrawer.ClearAsync();
        foreach (var wall in walls)
        {
            await _polygonDrawer.DrawAsync(wall);
        }
        foreach (var line in lines)
        {
            await _polygonDrawer.DrawAsync(line);
        }
        await _polygonDrawer.RenderAsync();

        _rectangle = Rectangle.RotateRectangle(_rectangle, 0.5f);
    }
}