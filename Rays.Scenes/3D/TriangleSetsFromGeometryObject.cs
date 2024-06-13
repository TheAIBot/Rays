using Rays._3D;
using Rays.GeometryLoaders.Geometry;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO.Compression;
using System.Numerics;

namespace Rays.Scenes;

public sealed class TriangleSetsFromGeometryObject : ITriangleSetsFromGeometryObject
{
    public ISubDividableTriangleSet[] Load(string zippedGeometryFilePath)
    {
        string geometryFolderPath = UnZipFileToDirectory(zippedGeometryFilePath);
        GeometryObject geometryObject = LoadGeometryObject(geometryFolderPath);

        var texturedTriangles = new List<ISubDividableTriangleSet>();
        foreach (var geometryModel in geometryObject.GeometryModels)
        {
            Triangle[] singleColorVertices = geometryModel.GetOneColorVertices()
                                                          .Select(x => new Triangle(ToVector3(x.CornerA.Value), ToVector3(x.CornerB.Value), ToVector3(x.CornerC.Value)))
                                                          .ToArray();
            if (singleColorVertices.Length != 0)
            {
                if (geometryModel.Material.Diffusion.Color == null)
                {
                    throw new InvalidOperationException("Neither diffuse texture or color was defined.");
                }

                var color = new Vector3(geometryModel.Material.Diffusion.Color.Value.X * 255,
                                        geometryModel.Material.Diffusion.Color.Value.Y * 255,
                                        geometryModel.Material.Diffusion.Color.Value.Z * 255);
                texturedTriangles.Add(new SimpleColoredTriangles(singleColorVertices, new Triangle(color, color, color)));
            }

            if (geometryModel.Material.Diffusion.TextureMapFileName == null)
            {
                continue;
            }

            Triangle[] texturedVertices = geometryModel.GetTexturedVertices()
                                                       .Select(x => new Triangle(ToVector3(x.CornerA.Value), ToVector3(x.CornerB.Value), ToVector3(x.CornerC.Value)))
                                                       .ToArray();
            if (texturedVertices.Length == 0)
            {
                continue;
            }

            Triangle[] triangleTextureCoordinates = geometryModel.GetTextureCoordinatesAsTriangles()
                                                                 .Select(x => new Triangle(x.CornerA.Coordinate, x.CornerB.Coordinate, x.CornerC.Coordinate))
                                                                 .ToArray();
            Image<Rgba32> texture = Image.Load<Rgba32>(geometryModel.Material.Diffusion.TextureMapFileName);
            texturedTriangles.Add(new TexturedTriangles(texturedVertices, triangleTextureCoordinates, texture));
        }

        return texturedTriangles.ToArray();
    }

    private static string UnZipFileToDirectory(string zipFilePath)
    {
        string directoryPath = Path.GetDirectoryName(zipFilePath)!;
        string zipFileName = Path.GetFileNameWithoutExtension(zipFilePath);
        string unzipDirectoryPath = $"{Path.Combine(directoryPath, zipFileName)}-unzipped";
        ZipFile.ExtractToDirectory(zipFilePath, unzipDirectoryPath, true);

        return unzipDirectoryPath;
    }

    private static GeometryObject LoadGeometryObject(string geometryObjectFolderPath)
    {
        string objectFilePath = Directory.GetFiles(geometryObjectFolderPath, "*.obj", new EnumerationOptions()
        {
            RecurseSubdirectories = true
        }).Single();

        return GeometryObjectBuilder.CreateFromFile(objectFilePath);
    }

    private static Vector3 ToVector3(Vector4 vector)
    {
        return new Vector3(vector.X, vector.Y, vector.Z);
    }
}
