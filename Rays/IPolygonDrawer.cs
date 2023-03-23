using Rays.Polygons;
using System.Numerics;

namespace Rays;

public interface IPolygonDrawer
{
    Vector2 Size { get; }
    Task DrawAsync(Rectangle rectangle);
    Task DrawAsync(Wall Wall);
    Task DrawAsync(Line line);

    Task ClearAsync();

    Task RenderAsync();
}