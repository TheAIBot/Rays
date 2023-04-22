using Rays;
using Rays.Polygons;
using System.Numerics;
using System.Text;

namespace ConsoleRays;

internal sealed class ConsolePolygonDrawer : IPolygonDrawer
{
    private readonly char[,] Screen = new char[100, 30];

    public Point Size { get; } = new Point(100, 30);

    public Task DrawAsync(Rectangle rectangle)
    {
        DrawLine(rectangle.BottomLeft, rectangle.Horizontal);
        DrawLine(rectangle.BottomLeft + rectangle.Vertical, rectangle.Horizontal);
        DrawLine(rectangle.BottomLeft, rectangle.Vertical);
        DrawLine(rectangle.BottomLeft + rectangle.Horizontal, rectangle.Vertical);

        return Task.CompletedTask;
    }

    public Task DrawAsync(Wall wall)
    {
        return DrawAsync(wall.Line);
    }

    public Task DrawAsync(Line line)
    {
        DrawLine(line.Start, line.Direction);

        return Task.CompletedTask;
    }

    private void DrawLine(Vector2 start, Vector2 direction)
    {
        float xDistance = direction.X;
        if (xDistance != 0)
        {
            int xSteps = (int)Math.Abs(xDistance);
            Vector2 xStepDirection = direction / xSteps;
            DrawLine(start, xSteps, xStepDirection);
        }

        float yDistance = direction.Y;
        if (yDistance != 0)
        {
            int ySteps = (int)Math.Abs(yDistance);
            Vector2 yStepDirection = direction / ySteps;
            DrawLine(start, ySteps, yStepDirection);
        }
    }

    private void DrawLine(Vector2 start, int steps, Vector2 stepDirection)
    {
        Vector2 position = start;
        for (int i = 0; i <= steps; i++)
        {
            if (WithinScreen(position))
            {
                Screen[(int)position.X, (int)position.Y] = '#';
            }

            position += stepDirection;
        }
    }

    public Task DrawPixelAsync(int x, int y, Color _)
    {
        if (!WithinScreen(new Vector2(x, y)))
        {
            return Task.CompletedTask;
        }

        Screen[x, y] = '#';
        return Task.CompletedTask;
    }

    private bool WithinScreen(Vector2 point)
    {
        return point.X >= 0 && point.X < Screen.GetLength(0) &&
               point.Y >= 0 && point.Y < Screen.GetLength(1);
    }

    public Task ClearAsync()
    {
        for (int y = 0; y < Screen.GetLength(1); y++)
        {
            for (int x = 0; x < Screen.GetLength(0); x++)
            {
                Screen[x, y] = ' ';
            }
        }

        return Task.CompletedTask;
    }

    public Task RenderAsync()
    {
        var sBuilder = new StringBuilder();
        for (int y = Screen.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < Screen.GetLength(0); x++)
            {
                sBuilder.Append(Screen[x, y]);
            }
            sBuilder.AppendLine();
        }

        Console.WriteLine(sBuilder.ToString());
        return Task.CompletedTask;
    }
}
