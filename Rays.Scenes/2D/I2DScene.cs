namespace Rays.Scenes;

public interface I2DScene
{
    Task RenderAsync(CancellationToken cancellationToken);
}