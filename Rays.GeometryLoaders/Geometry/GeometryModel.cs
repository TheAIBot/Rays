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
            for (int i = 0; i < face.VertexIndexes.Length - Triangle<Vertex>.EdgeCount; i++)
            {
                int vertexIndex1 = face.VertexIndexes[i + 0] - 1;
                int vertexIndex2 = face.VertexIndexes[i + 1] - 1;
                int vertexIndex3 = face.VertexIndexes[i + 2] - 1;

                Vertex vertex1 = geometryObject.Vertices[vertexIndex1];
                Vertex vertex2 = geometryObject.Vertices[vertexIndex2];
                Vertex vertex3 = geometryObject.Vertices[vertexIndex3];

                yield return new Triangle<Vertex>(vertex1, vertex2, vertex3);
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