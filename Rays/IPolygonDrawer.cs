using Rays.Polygons;
using Rectangle = Rays.Polygons.Rectangle;

namespace Rays;

public interface IPolygonDrawer
{
    Point Size { get; }
    Task DrawAsync(Rectangle rectangle);
    Task DrawAsync(Wall Wall);
    Task DrawAsync(Line line);

    void DrawPixel(int x, int y, Color color);
    ValueTask DrawPixelAsync(int x, int y, Color color);

    Task ClearAsync();

    Task RenderAsync(CancellationToken cancellationToken);
}

public readonly record struct Color(byte Red, byte Green, byte Blue, byte Alpha)
{
    public const int Channels = 4;
    public Color(int red, int green, int blue, int alpha) : this((byte)red, (byte)green, (byte)blue, (byte)alpha) { }

    public static Color Blend(Color color1, Color color2, byte alpha) =>
        new(
            (byte)((color1.Red * (byte.MaxValue - alpha) + color2.Red * alpha) / byte.MaxValue),
            (byte)((color1.Green * (byte.MaxValue - alpha) + color2.Green * alpha) / byte.MaxValue),
            (byte)((color1.Blue * (byte.MaxValue - alpha) + color2.Blue * alpha) / byte.MaxValue),
            (byte)((color1.Alpha * (byte.MaxValue - alpha) + color2.Alpha * alpha) / byte.MaxValue));
}

public readonly record struct Point(int X, int Y)
{
    public static Point operator /(Point p1, int divider) => new(p1.X / divider, p1.Y / divider);
}