using CommunityToolkit.HighPerformance;
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
                line = line.Slice(0, commentStartIndex);
            }

            var lineTokens = line.Tokenize(' ');
            if (!lineTokens.MoveNext())
            {
                throw new InvalidOperationException("Unexpected end of line.");
            }

            switch (lineTokens.Current)
            {
                case Vertex.LinePrefix:
                    {
                        if (!lineTokens.MoveNext())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        objectBuilder.vertices.Add(Vertex.Parse(lineTokens));
                    }
                    break;
                case VertexNormal.LinePrefix:
                    {
                        if (!lineTokens.MoveNext())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        objectBuilder.normals.Add(VertexNormal.Parse(lineTokens));
                    }
                    break;
                case TextureCoordinate.LinePrefix:
                    {
                        if (!lineTokens.MoveNext())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        objectBuilder.textureCoordinates.Add(TextureCoordinate.Parse(lineTokens));
                    }
                    break;
                case "g":
                    {
                        if (!lineTokens.MoveNext())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        currentModelBuilder = objectBuilder.AddGeometryModel(lineTokens.Current.ToString());
                    }
                    break;
                case Face.LinePrefix:
                    {
                        if (!lineTokens.MoveNext())
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
                        if (!lineTokens.MoveNext())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }

                        string? folderPath = Path.GetDirectoryName(filePath);
                        if (folderPath == null)
                        {
                            throw new InvalidOperationException("Failed to get the directory the .obj file resides in.");
                        }

                        string materialFileName = Path.Combine(folderPath, lineTokens.Current.ToString());
                        if (!File.Exists(materialFileName))
                        {
                            throw new FileNotFoundException($"No materials file with the name {materialFileName} exists.");
                        }

                        foreach (var material in Material.CreateFromString(File.ReadAllText(materialFileName)))
                        {
                            materials.Add(material.Name, material);
                        }
                    }
                    break;
                case "usemtl":
                    {
                        if (!lineTokens.MoveNext())
                        {
                            throw new InvalidOperationException("Unexpected end of line.");
                        }
                        if (currentModelBuilder == null)
                        {
                            throw new InvalidOperationException("Object group must be defined before setting material.");
                        }
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
        return new GeometryObject(vertices.ToArray(),
                                  normals.ToArray(),
                                  textureCoordinates.ToArray(),
                                  modelBuilders.Select(x => x.Build()).ToArray());
    }
}
