namespace Rays.GeometryLoaders;

public sealed record GeometryObject(Vertex[] Vertices, VertexNormal[] Normals, TextureCoordinate[] TextureCoordinates, GeometryModel[] GeometryModels);
