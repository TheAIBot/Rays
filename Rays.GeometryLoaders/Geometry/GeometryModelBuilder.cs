using System.Text;
using System.Windows.Markup;

namespace Rays.GeometryLoaders.Geometry;

internal sealed class GeometryModelBuilder
{
    private readonly List<Face> faces = new();
    private readonly string modelName;

    public GeometryModelBuilder(string modelName)
    {
        this.modelName = modelName;
    }

    internal void AddFace(Face face)
    {
        faces.Add(face);
    }

    internal GeometryModel Build()
    {
        return new GeometryModel(modelName, faces.ToArray());
    }
}