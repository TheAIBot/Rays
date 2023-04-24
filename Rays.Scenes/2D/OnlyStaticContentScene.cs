using Rays.Polygons;
using System.Numerics;

namespace Rays.Scenes;

internal sealed class OnlyStaticContentScene : I2DScene
{
    private readonly IPolygonDrawer _polygonDrawer;
    private readonly List<Wall> _staticWalls;

    public OnlyStaticContentScene(IPolygonDrawer polygonDrawer, List<Wall> staticWalls)
    {
        _polygonDrawer = polygonDrawer;
        _staticWalls = staticWalls;
    }

    public async Task RenderAsync()
    {
        var ray = new Ray(new Vector2(70, 90), Vector2.Normalize(new Vector2(-1, -1.5f)));
        List<Line> lines = Ray.GetRayPath(ray, 9, _staticWalls);

        await _polygonDrawer.ClearAsync();
        foreach (var wall in _staticWalls)
        {
            await _polygonDrawer.DrawAsync(wall);
        }
        foreach (var line in lines)
        {
            await _polygonDrawer.DrawAsync(line);
        }
        await _polygonDrawer.RenderAsync();
    }
}