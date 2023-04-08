using CommunityToolkit.HighPerformance;

namespace Rays.GeometryLoaders.Geometry;

public sealed class GeometryObjectBuilder
{
    private readonly List<Vertex> vertices = new();
    private readonly List<VertexNormal> normals = new();
    private readonly List<TextureCoordinate> textureCoordinates = new();
    private readonly List<GeometryModelBuilder> modelBuilders = new();

    public static GeometryObject CreateFromString(string content)
    {
        return CreateFromStream(new StringReader(content));
    }

    private static GeometryObject CreateFromStream(TextReader stream)
    {
        var objectBuilder = new GeometryObjectBuilder();

        //State
        GeometryModelBuilder? currentModelBuilder = null;

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
