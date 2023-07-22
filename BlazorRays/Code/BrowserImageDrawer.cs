using Microsoft.JSInterop;
using Rays;
using Rays.Polygons;

namespace BlazorRays.Code
{
    public sealed class BrowserImageDrawer : IPolygonDrawer
    {
        private readonly IJSRuntime _runtime;
        private readonly DotNetStreamReference _imageStream;
        private Rgba32[] _writeImageData;
        private Rgba32[] _readImageData;
        private Image<Rgba32> _image;
        private Task? _sendingToBrowser;

        public Rays.Point Size { get; }

        public BrowserImageDrawer(Rays.Point imageSize, IJSRuntime runtime)
        {
            Size = imageSize;
            _writeImageData = new Rgba32[Size.X * Size.Y];
            _readImageData = new Rgba32[Size.X * Size.Y];
            _runtime = runtime;
            _image = Image.WrapMemory<Rgba32>(_writeImageData, Size.X, Size.Y);
            _imageStream = new DotNetStreamReference(new MemoryStream(), true);
        }

        public Task ClearAsync()
        {
            Array.Clear(_writeImageData);
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

        public ValueTask DrawPixelAsync(int x, int y, Rays.Color color)
        {
            _image[x, y] = new Rgba32(color.Red, color.Green, color.Blue, color.Alpha);
            return ValueTask.CompletedTask;
        }

        public async Task RenderAsync()
        {
            if (_sendingToBrowser != null)
            {
                await _sendingToBrowser;
            }

            SwapImageBuffers();
            _sendingToBrowser = Task.Factory.StartNew(async () =>
            {
                Image<Rgba32> image = Image.WrapMemory<Rgba32>(_readImageData, Size.X, Size.Y);
                _imageStream.Stream.Seek(0, SeekOrigin.Begin);
                image.SaveAsPng(_imageStream.Stream);
                _imageStream.Stream.Seek(0, SeekOrigin.Begin);
                await _runtime.InvokeVoidAsync("setImage", "image", _imageStream).AsTask();
            });
        }

        public void SwapImageBuffers()
        {
            var a = _writeImageData;
            _writeImageData = _readImageData;
            _readImageData = a;

            _image = Image.WrapMemory<Rgba32>(_writeImageData, Size.X, Size.Y);
        }
    }
}
