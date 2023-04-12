using Rays.Polygons;
using System.Numerics;

namespace Rays;

public interface IPolygonDrawer
{
    Vector2 Size { get; }
    Task DrawAsync(Rectangle rectangle);
    Task DrawFillAsync(Rectangle rectangle);
    Task DrawAsync(Wall Wall);
    Task DrawAsync(Line line);

    Task SetFillColorAsync(Color color);

    Task ClearAsync();

    Task RenderAsync();
}

public readonly record struct Color(byte Red, byte Green, byte Blue, byte Alpha)
{
    public const int Channels = 4;
    public Color(int red, int green, int blue, int alpha) : this((byte)red, (byte)green, (byte)blue, (byte)alpha) { }
}