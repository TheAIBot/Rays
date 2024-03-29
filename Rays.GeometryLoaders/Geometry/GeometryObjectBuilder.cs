﻿using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;
using Rays.GeometryLoaders.Materials;

namespace Rays.GeometryLoaders.Geometry;

public sealed class GeometryObjectBuilder
{
    private readonly List<Vertex> vertices = new();
    private readonly List<VertexNormal> normals = new();
    private readonly List<TextureCoordinate> textureCoordinates = new();
    private readonly List<GeometryModelBuilder> modelBuilders = new();

    public static GeometryObject CreateFromFile(string filePath)
    {
        string? folderPath = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Failed to get the directory the .obj file resides in.");
        using TextReader stream = new StreamReader(filePath);
        var objectBuilder = new GeometryObjectBuilder();

        //State
        GeometryModelBuilder? currentModelBuilder = null;
        var materials = new Dictionary<string, Material>();

        ReadOnlySpan<char> line;
        while ((line = stream.ReadLine()) != null)
        {
            //Skip empty lines
            if (line.Length == 0)
            {
                continue;
            }

            int commentStartIndex = line.IndexOf('#');
            //Skip line if it starts with a comment
            if (commentStartIndex == 0)
            {
                continue;
            }

            //Remove comment from end of line
            if (commentStartIndex != -1)
            {
                line = line[..commentStartIndex];
            }

            var lineTokens = line.Tokenize(' ');
            if (!lineTokens.MoveNextNonEmpty())
            {
                throw new InvalidOperationException("Unexpected end of line.");
            }

            switch (lineTokens.Current)
            {
                case Vertex.LinePrefix:
                    {
                        if (!lineTokens.MoveNextNonEmpty())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        objectBuilder.vertices.Add(Vertex.Parse(lineTokens));
                    }
                    break;
                case VertexNormal.LinePrefix:
                    {
                        if (!lineTokens.MoveNextNonEmpty())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        objectBuilder.normals.Add(VertexNormal.Parse(lineTokens));
                    }
                    break;
                case TextureCoordinate.LinePrefix:
                    {
                        if (!lineTokens.MoveNextNonEmpty())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        objectBuilder.textureCoordinates.Add(TextureCoordinate.Parse(lineTokens));
                    }
                    break;
                case "g":
                    {
                        if (!lineTokens.MoveNextNonEmpty())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        currentModelBuilder = objectBuilder.AddGeometryModel(lineTokens.Current.ToString());
                    }
                    break;
                case Face.LinePrefix:
                    {
                        if (!lineTokens.MoveNextNonEmpty())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        if (currentModelBuilder == null)
                        {
                            throw new InvalidOperationException("Object group must be defined before first face.");
                        }

                        currentModelBuilder.AddFace(Face.Parse(lineTokens));
                    }
                    break;
                case "mtllib":
                    {
                        if (!lineTokens.MoveNextNonEmpty())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }

                        string materialFileName = Path.Combine(folderPath, lineTokens.Current.ToString());
                        if (!File.Exists(materialFileName))
                        {
                            throw new FileNotFoundException($"No materials file with the name {materialFileName} exists.");
                        }

                        foreach (var material in Material.CreateFromString(File.ReadAllText(materialFileName), folderPath))
                        {
                            materials.Add(material.Name, material);
                        }
                    }
                    break;
                case "usemtl":
                    {
                        if (!lineTokens.MoveNextNonEmpty())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        // Inject default model only if none were defined yet.
                        // Apparently it is valid to not define a geometry object but
                        // we store everything in geometry objects here which is why
                        // a default is created for such cases.
                        currentModelBuilder ??= objectBuilder.AddGeometryModel("__Default__");
                        if (!materials.TryGetValue(lineTokens.Current.ToString(), out Material? material))
                        {
                            throw new InvalidOperationException($"No material with the name {lineTokens.Current} exist.");
                        }

                        currentModelBuilder.SetMaterial(material);
                    }
                    break;
                default:
                    //Skip any command that is not support yet
                    break;
            }
        }

        return objectBuilder.Build();
    }

    internal GeometryModelBuilder AddGeometryModel(string modelName)
    {
        var geometryModel = new GeometryModelBuilder(modelName);
        modelBuilders.Add(geometryModel);
        return geometryModel;
    }

    internal GeometryObject Build()
    {
        var geometryObject = new GeometryObject(vertices.ToArray(),
                                                normals.ToArray(),
                                                textureCoordinates.ToArray(),
                                                modelBuilders.Select(x => x.Build()).ToArray());
        foreach (var geometryModel in geometryObject.GeometryModels)
        {
            geometryModel.SetGeometryObject(geometryObject);
        }

        return geometryObject;
    }
}

internal static class SpanTokenizerExtensions
{
    public static bool MoveNextNonEmpty(this ref ReadOnlySpanTokenizer<char> spanTokenizer)
    {
        while (spanTokenizer.MoveNext())
        {
            if (!spanTokenizer.Current.IsEmpty)
            {
                return true;
            }
        }

        return false;
    }
}
