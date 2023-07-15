using Rays.Scenes;
using System.Reflection;

namespace BlazorRays;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddSingleton<CameraFactory>();
        builder.Services.AddSingleton<TriangleSetsFromGeometryObject>();
        builder.Services.AddDisplayableOption<ITriangleSetIntersectorFromGeometryObject, TriangleTreeFromGeometryObject>("Triangle Tree", true);
        builder.Services.AddDisplayableOption<ITriangleSetIntersectorFromGeometryObject, TriangleListFromGeometryObject>("Triangle List", false);
        builder.Services.AddDisplayableOption<I3DSceneGeometryObjectFactory, RayTracerFromGeometryObjectFactory>("Default", true);
        builder.Services.AddDisplayableOption<I3DSceneGeometryObjectFactory, DisplayDepthRayTracerFromGeometryObjectFactory>("Depth", false);

        foreach (var model in Directory.GetFiles(GetModelsFolderPath()))
        {
            builder.Services.AddSingleton(new GeometryObjectModelFile(model));
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }


        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }

    private static string GetModelPath(string modelFileName)
    {
        string executingFile = Assembly.GetExecutingAssembly().Location;
        string executingDirectory = Path.GetDirectoryName(executingFile)!;
        string modelsFolder = Path.Combine(executingDirectory, "Models");

        return Path.Combine(modelsFolder, modelFileName);
    }

    private static string GetModelsFolderPath()
    {
        string executingFile = Assembly.GetExecutingAssembly().Location;
        string executingDirectory = Path.GetDirectoryName(executingFile)!;
        return Path.Combine(executingDirectory, "Models");
    }

    private static void AddDisplayableOption<TInterface, TImplementation>(this IServiceCollection services, string displayName, bool isDefault)
        where TImplementation : class, TInterface
    {
        services.AddSingleton<TImplementation>();
        services.AddSingleton(x => new DisplayableOption<TInterface>(x.GetRequiredService<TImplementation>(), displayName, isDefault));
    }
}

public sealed record DisplayableOption<T>(T Value, string DisplayName, bool IsDefault);

public sealed record GeometryObjectModelFile(string ModelFileName)
{
    public string CleanFileName => Path.GetFileNameWithoutExtension(ModelFileName);
}

public sealed record SceneSettings(GeometryObjectModelFile GeometryModel, ITriangleSetIntersectorFromGeometryObject TriangleSetIntersectorFactory, I3DSceneGeometryObjectFactory SceneFactory);