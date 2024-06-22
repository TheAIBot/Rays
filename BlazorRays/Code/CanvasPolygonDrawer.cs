using Blazor.Extensions.Canvas.Canvas2D;
using Rays;
using Rays.Polygons;
using System.Numerics;

namespace BlazorRays.Code
{
    public sealed class CanvasPolygonDrawer : IPolygonDrawer
    {
        private readonly Canvas2DContext DrawingContext;

        public Rays.Point Size { get; }

        public CanvasPolygonDrawer(Canvas2DContext canvas2DContext, Rays.Point canvasSize)
        {
            DrawingContext = canvas2DContext;
            Size = canvasSize;
        }

        public async Task DrawAsync(Rays.Polygons.Rectangle rectangle)
        {
            await DrawingContext.BeginPathAsync();

            await DrawingContext.MoveToAsync(rectangle.TopLeft.X, rectangle.TopLeft.Y);
            await DrawingContext.LineToAsync(rectangle.TopRight.X, rectangle.TopRight.Y);
            await DrawingContext.LineToAsync(rectangle.BottomRight.X, rectangle.BottomRight.Y);
            await DrawingContext.LineToAsync(rectangle.BottomLeft.X, rectangle.BottomLeft.Y);
            await DrawingContext.LineToAsync(rectangle.TopLeft.X, rectangle.TopLeft.Y);

            await DrawingContext.StrokeAsync();

            foreach (var wall in rectangle.GetAsWalls())
            {
                await DrawWallNormal(wall);
            }
        }

        public async Task DrawAsync(Wall wall)
        {
            await DrawAsync(wall.Line);
            await DrawWallNormal(wall);
        }

        private async Task DrawWallNormal(Wall wall)
        {
            await DrawingContext.BeginPathAsync();

            Vector2 distance = wall.Line.End - wall.Line.Start;
            Vector2 center = wall.Line.Start + distance / 2;
            Vector2 normalFromCenter = center + wall.Normal * 5;
            await DrawingContext.MoveToAsync(center.X, center.Y);
            await DrawingContext.LineToAsync(normalFromCenter.X, normalFromCenter.Y);

            await DrawingContext.StrokeAsync();
        }

        public async Task DrawAsync(Line line)
        {
            await DrawingContext.BeginPathAsync();

            await DrawingContext.MoveToAsync(line.Start.X, line.Start.Y);
            await DrawingContext.LineToAsync(line.End.X, line.End.Y);

            await DrawingContext.StrokeAsync();
        }

        public void DrawPixel(int x, int y, Rays.Color color)
        {
            DrawPixelAsync(x, y, color).AsTask().GetAwaiter().GetResult();
        }

        public async ValueTask DrawPixelAsync(int x, int y, Rays.Color color)
        {
            await DrawingContext.SetFillStyleAsync(ToCanvasColor(color));
            await DrawingContext.FillRectAsync(x, y, 1, 1);
        }

        private static string ToCanvasColor(Rays.Color color)
        {
            Span<byte> colorBytes = stackalloc byte[Rays.Color.Channels];
            colorBytes[0] = color.Red;
            colorBytes[1] = color.Green;
            colorBytes[2] = color.Blue;
            colorBytes[3] = color.Alpha;

            return $"#{Convert.ToHexString(colorBytes)}";
        }

        public async Task ClearAsync()
        {
            //Need to bactch the commands manually when using server
            //side blazor.
            await DrawingContext.BeginBatchAsync();

            //Set background color of the canvas
            await DrawingContext.ClearRectAsync(0, 0, Size.X, Size.Y);

            //Horizontal flip. First set transformation matrix and
            //then move the view into negative y since it was flipped
            //into that space.
            await DrawingContext.SetTransformAsync(1, 0, 0, -1, 0, 0);
            await DrawingContext.TranslateAsync(0, -Size.Y);
        }

        public Task RenderAsync(CancellationToken cancellationToken)
        {
            return DrawingContext.EndBatchAsync();
        }
    }
}
