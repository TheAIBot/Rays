using CommunityToolkit.HighPerformance;
using System.Globalization;
using CommunityToolkit.HighPerformance.Enumerables;

namespace Rays.GeometryLoaders;

public readonly record struct Face(int[] VertexIndexes, int[]? TextureCoordinateIndexes, int[]? NormalIndexes)
{
    internal const string LinePrefix = "f";

    internal static Face Parse(ReadOnlySpanTokenizer<char> lineTokens)
    {
        var vertexIndexes = new List<int>();
        var textureCoordinateIndexes = new List<int>();
        var normalIndexes = new List<int>();
        do
        {
            var faceElements = lineTokens.Current;
            ReadOnlySpanTokenizer<char> elementTokens = faceElements.Tokenize('/');

            if (!elementTokens.MoveNext())
            {
                throw new InvalidOperationException();
            }
            vertexIndexes.Add(int.Parse(elementTokens.Current, CultureInfo.InvariantCulture));

            if (!elementTokens.MoveNext())
            {
                continue;
            }
            if (elementTokens.Current.Length != 0)
            {
                textureCoordinateIndexes.Add(int.Parse(elementTokens.Current, CultureInfo.InvariantCulture));
            }

            if (!elementTokens.MoveNext())
            {
                continue;
            }
            normalIndexes.Add(int.Parse(elementTokens.Current, CultureInfo.InvariantCulture));
        } while (lineTokens.MoveNext());

        return new Face(vertexIndexes.ToArray(), 
                        textureCoordinateIndexes.Count > 0 ? textureCoordinateIndexes.ToArray() : null,
                        normalIndexes.Count > 0 ? normalIndexes.ToArray() : null);
    }
}
