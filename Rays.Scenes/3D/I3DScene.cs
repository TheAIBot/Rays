namespace Rays.Scenes;

public interface I3DScene
{
    Camera Camera { get; }

    Task RenderAsync();
}