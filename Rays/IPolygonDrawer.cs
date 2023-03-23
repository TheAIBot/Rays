using Rays.Polygons;

namespace Rays;

public interface IPolygonDrawer
{
    Task DrawAsync(Rectangle rectangle);
    Task DrawAsync(Wall Wall);
    Task DrawAsync(Line line);

    Task ClearAsync();

    Task RenderAsync();
}