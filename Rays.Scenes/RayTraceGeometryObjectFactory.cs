using Rays._3D;
using Rays.GeometryLoaders.Geometry;
using System.IO.Compression;
using System.Numerics;

namespace Rays.Scenes;

public sealed class RayTraceGeometryObjectFactory : ISceneFactory
{
    private readonly string _zippedGeometryFilePath;

    public RayTraceGeometryObjectFactory(string zippedGeometryFilePath)
    {
        _zippedGeometryFilePath = zippedGeometryFilePath;
    }

    public IScene Create(IPolygonDrawer polygonDrawer)
    {
        string geometryFolderPath = UnZipFileToDirectory(_zippedGeometryFilePath);
        GeometryObject geometryObject = LoadGeometryObject(geometryFolderPath);

        var texturedTriangles = new List<ITexturedTriangles>();
        foreach (var geometryModel in geometryObject.GeometryModels)
        {
            Triangle[] triangles = geometryModel.GetVerticesAsTriangles()
                                                .Select(x => new Triangle(ToVector3(x.CornerA.Value), ToVector3(x.CornerB.Value), ToVector3(x.CornerC.Value)))
                                                .ToArray();

            if (geometryModel.Material.Diffusion.TextureMapFileName == null)
            {
                if (geometryModel.Material.Diffusion.Color == null)
                {
                    throw new InvalidOperationException("Neither diffuse texture or color was defined.");
                }

                Vector3 color = new Vector3(geometryModel.Material.Diffusion.Color.Value.X * 255,
                                            geometryModel.Material.Diffusion.Color.Value.Y * 255,
                                            geometryModel.Material.Diffusion.Color.Value.Z * 255);
                texturedTriangles.Add(new SimpleColoredTriangles(triangles, new Triangle(color, color, color)));
                continue;
            }

            Triangle<TextureCoordinate>[] triangleTextureCoordinates = geometryModel.GetTextureCoordinatesAsTriangles()
                                                                                    .Select(x => new Triangle<TextureCoordinate>(x.CornerA, x.CornerB, x.CornerC))
                                                                                    .ToArray();
            Image<Rgba32> texture = Image.Load<Rgba32>(geometryModel.Material.Diffusion.TextureMapFileName);
            texturedTriangles.Add(new TexturedTriangles(triangles, triangleTextureCoordinates, texture));
        }

        return new RayTracer(polygonDrawer, texturedTriangles.ToArray());
    }

    private static string UnZipFileToDirectory(string zipFilePath)
    {
        string unzipDirectoryPath = $"{Path.GetFileNameWithoutExtension(zipFilePath)}-unzipped";
        ZipFile.ExtractToDirectory(zipFilePath, unzipDirectoryPath, true);

        return unzipDirectoryPath;
    }

    private static GeometryObject LoadGeometryObject(string geometryObjectFolderPath)
    {
        string objectFilePath = Directory.GetFiles(geometryObjectFolderPath, "*.obj", new EnumerationOptions() { RecurseSubdirectories = true }).Single();

        return GeometryObjectBuilder.CreateFromFile(objectFilePath);
    }

    private static Vector3 ToVector3(Vector4 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
}