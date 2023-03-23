using Rays;
using Rays.Scenes;

namespace ConsoleRays;

internal sealed class Program
{
    public static async Task Main(string[] args)
    {
        IPolygonDrawer polygonDrawer = new ConsolePolygonDrawer();
        IScene scene = new SpinningRectangleFactory().Create(polygonDrawer);

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        while (await timer.WaitForNextTickAsync())
        {
            await scene.RenderAsync();
        }

        Console.ReadLine();
    }
}
