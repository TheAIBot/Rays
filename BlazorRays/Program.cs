using Rays.Scenes;
using System.Reflection;

namespace BlazorRays;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.Add3DRayTracerSceneServices();
        builder.Services.AddSingleton<SceneSettings>();
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
}

public sealed record GeometryObjectModelFile(string ModelFileName)
{
    public string CleanFileName => Path.GetFileNameWithoutExtension(ModelFileName);
}

public sealed record SceneSettings(GeometryObjectModelFile GeometryModel, ITriangleSetIntersectorFromGeometryObject TriangleSetIntersectorFactory, I3DSceneGeometryObjectFactory SceneFactory);