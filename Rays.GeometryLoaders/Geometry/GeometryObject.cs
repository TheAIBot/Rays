namespace Rays.GeometryLoaders.Geometry;

public sealed record GeometryObject(Vertex[] Vertices, VertexNormal[] Normals, TextureCoordinate[] TextureCoordinates, GeometryModel[] GeometryModels);
