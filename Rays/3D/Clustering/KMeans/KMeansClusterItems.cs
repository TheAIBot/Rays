using System.Numerics;

namespace Rays._3D;

public sealed record KMeansClusterItems<T>(T[] Items, Vector4[] Positions)
{
    public int Count => Items.Length;

    public static KMeansClusterItems<T> Create(IEnumerable<T> items, Func<T, Vector4> getPosition)
    {
        T[] itemsArray = items.ToArray();
        Vector4[] positions = itemsArray.Select(getPosition).ToArray();

        return new KMeansClusterItems<T>(itemsArray, positions);
    }
}
