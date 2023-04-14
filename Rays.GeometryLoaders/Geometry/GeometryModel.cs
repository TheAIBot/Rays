using Rays.GeometryLoaders.Materials;

namespace Rays.GeometryLoaders.Geometry;

public sealed class GeometryModel : IEquatable<GeometryModel>
{
    private GeometryObject geometryObject = null!;

    public string Name { get; }
    public Face[] Faces { get; }
    public Material Material { get; }

    public GeometryModel(string name, Face[] faces, Material material)
    {
        Name = name;
        Faces = faces;
        Material = material;
    }

    internal void SetGeometryObject(GeometryObject geometryObject)
    {
        this.geometryObject = geometryObject;
    }

    public IEnumerable<Triangle<Vertex>> GetVerticesAsTriangles()
    {
        foreach (var face in Faces)
        {
            int index1 = face.VertexIndexes[0] - 1;
            Vertex value1 = geometryObject.Vertices[index1];

            for (int i = 1; i < face.VertexIndexes.Length - 1; i++)
            {
                int index2 = face.VertexIndexes[i] - 1;
                int index3 = face.VertexIndexes[i + 1] - 1;

                Vertex value2 = geometryObject.Vertices[index2];
                Vertex value3 = geometryObject.Vertices[index3];

                yield return new Triangle<Vertex>(value1, value2, value3);
            }
        }
    }

    public IEnumerable<Triangle<TextureCoordinate>> GetTextureCoordinatesAsTriangles()
    {
        foreach (var face in Faces)
        {
            if (face.TextureCoordinateIndexes == null)
            {
                throw new InvalidOperationException();
            }

            int index1 = face.TextureCoordinateIndexes[0] - 1;
            TextureCoordinate value1 = geometryObject.TextureCoordinates[index1];

            for (int i = 1; i < face.TextureCoordinateIndexes.Length - 1; i++)
            {
                int index2 = face.TextureCoordinateIndexes[i] - 1;
                int index3 = face.TextureCoordinateIndexes[i + 1] - 1;

                TextureCoordinate value2 = geometryObject.TextureCoordinates[index2];
                TextureCoordinate value3 = geometryObject.TextureCoordinates[index3];

                yield return new Triangle<TextureCoordinate>(value1, value2, value3);
            }
        }
    }

    public bool Equals(GeometryModel? other)
    {
        if (other == null)
        {
            return false;
        }

        return Name.Equals(other.Name) &&
               Faces.Equals(other.Faces) &&
               Material.Equals(other.Material);
    }
}