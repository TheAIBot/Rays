using Rays;
using Rays.Polygons;
using System.Numerics;

namespace ConsoleRays;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        IPolygonDrawer polygonDrawer = new ConsolePolygonDrawer();


        var rectangle = new Rectangle(new Vector2(10, 10), new Vector2(10, 0), new Vector2(0, 10));
        var border = new Rectangle(new Vector2(0, 0), new Vector2(99, 0), new Vector2(0, 29));

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        while (await timer.WaitForNextTickAsync())
        {
            await polygonDrawer.ClearAsync();
            await polygonDrawer.DrawAsync(rectangle);
            await polygonDrawer.DrawAsync(border);
            await polygonDrawer.RenderAsync();

            rectangle = RotateRectangle(rectangle, 5);
        }

        Console.ReadLine();
    }

    private static Rectangle RotateRectangle(Rectangle rectangle, float angleInDeg)
    {
        float angle = DegreesToRadians(angleInDeg);
        Matrix3x2 rotator = Matrix3x2.CreateRotation(angle);
        Vector2 toCenter = (rectangle.Horizontal + rectangle.Vertical) / 2;
        Vector2 centerPosition = rectangle.BottomLeft + toCenter;
        Vector2 gre = Vector2.Transform(toCenter, rotator);
        Vector2 newPos = centerPosition - gre;

        return new Rectangle(newPos, Vector2.Transform(rectangle.Horizontal, rotator), Vector2.Transform(rectangle.Vertical, rotator));
    }

    private static float DegreesToRadians(float degrees)
    {
        return degrees * (MathF.PI / 180);
    }
}
