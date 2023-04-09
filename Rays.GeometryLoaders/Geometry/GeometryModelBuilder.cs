using Rays.GeometryLoaders.Materials;

namespace Rays.GeometryLoaders.Geometry;

internal sealed class GeometryModelBuilder
{
    private readonly string modelName;
    private readonly List<Face> faces = new();
    private Material? material = null;

    public GeometryModelBuilder(string modelName)
    {
        this.modelName = modelName;
    }

    internal void AddFace(Face face)
    {
        faces.Add(face);
    }

    internal void SetMaterial(Material material)
    {
        this.material = material;
    }

    internal GeometryModel Build()
    {
        if (material == null)
        {
            throw new InvalidOperationException($"Missing material for {modelName} geometry group.");
        }

        return new GeometryModel(modelName, faces.ToArray(), material);
    }
}