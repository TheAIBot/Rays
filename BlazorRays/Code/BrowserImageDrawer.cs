using Microsoft.JSInterop;
using Rays;
using Rays.Polygons;
using System.Runtime.InteropServices;

namespace BlazorRays.Code
{
    public sealed class BrowserImageDrawer : IPolygonDrawer
    {
        private readonly IJSRuntime _runtime;
        private byte[] _writeImageData;
        private byte[] _readImageData;
        private Image<Rgba32> _image;
        private Task? _sendingToBrowser;

        public Rays.Point Size { get; }

        public BrowserImageDrawer(Rays.Point imageSize, IJSRuntime runtime)
        {
            Size = imageSize;
            _writeImageData = new byte[Size.X * Size.Y * Marshal.SizeOf<Rgba32>()];
            _readImageData = new byte[Size.X * Size.Y * Marshal.SizeOf<Rgba32>()];
            _runtime = runtime;
            _image = Image.WrapMemory<Rgba32>(_writeImageData, Size.X, Size.Y);
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
                await _runtime.InvokeVoidAsync("setImage", _readImageData);
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
