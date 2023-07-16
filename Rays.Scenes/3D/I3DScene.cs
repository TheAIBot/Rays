namespace Rays.Scenes;

public interface I3DScene
{
    Camera Camera { get; }
    SceneInformation Information { get; }

    Task RenderAsync(CancellationToken cancellationToken);
}