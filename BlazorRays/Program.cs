using Rays.Scenes;
using System.Reflection;

namespace Company.WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<I3DSceneFactory>(_ => new RayTraceGeometryObjectFactory(GetModelPath("Airplane.zip")));

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
    }
}