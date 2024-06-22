using Rays;
using Rays.Scenes;

namespace ConsoleRays;

internal sealed class Program
{
    public static async Task Main(string[] _)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        IPolygonDrawer polygonDrawer = new ConsolePolygonDrawer();
        I2DScene scene = new SpinningRectangleFactory().Create(polygonDrawer);

        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        while (await timer.WaitForNextTickAsync())
        {
            await scene.RenderAsync(cancellationTokenSource.Token);
        }

        Console.ReadLine();
    }
}
