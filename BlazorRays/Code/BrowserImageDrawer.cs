using Microsoft.JSInterop;
using Rays;
using Rays.Polygons;

namespace BlazorRays.Code
{
    public sealed class BrowserImageDrawer : IPolygonDrawer
    {
        private readonly Rgba32[] _imageData;
        private readonly Image<Rgba32> _image;
        private DotNetStreamReference _imageStream;
        private readonly IJSRuntime _runtime;

        public Rays.Point Size
        { get; }

        public BrowserImageDrawer(Rays.Point imageSize, IJSRuntime runtime)
        {
            Size = imageSize;
            _imageData = new Rgba32[Size.X * Size.Y];
            _image = Image.WrapMemory<Rgba32>(_imageData, Size.X, Size.Y);
            _imageStream = new DotNetStreamReference(new MemoryStream(), true);
            _runtime = runtime;
        }

        public Task ClearAsync()
        {
            Array.Clear(_imageData);
            return Task.CompletedTask;
        }

        public Task DrawAsync(Rays.Polygons.Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public Task DrawAsync(Wall Wall)
        {
            throw new NotImplementedException();
        }

        public Task DrawAsync(Line line)
        {
            throw new NotImplementedException();
        }

        public Task DrawPixelAsync(int x, int y, Rays.Color color)
        {
            _image[x, y] = new Rgba32(color.Red, color.Green, color.Blue, color.Alpha);
            return Task.CompletedTask;
        }

        public async Task RenderAsync()
        {
            _imageStream.Stream.Seek(0, SeekOrigin.Begin);
            _image.SaveAsPng(_imageStream.Stream);
            _imageStream.Stream.Seek(0, SeekOrigin.Begin);
            await _runtime.InvokeVoidAsync("setImage", "image", _imageStream);
        }
    }
}
