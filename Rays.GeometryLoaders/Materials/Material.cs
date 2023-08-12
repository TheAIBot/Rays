namespace Rays.GeometryLoaders.Materials;

public sealed record Material(string Name, Ambient Ambient, Diffusion Diffusion, Specular Specular, Opacity Opacity, Deformity Deformity)
{
    public static IEnumerable<Material> CreateFromString(string content, string folderPath)
    {
        return CreateFromStream(new StringReader(content), folderPath);
    }

    private static IEnumerable<Material> CreateFromStream(TextReader stream, string folderPath)
    {
        var allLines = new Queue<string>();
        string? line;
        while ((line = stream.ReadLine()) != null)
        {
            allLines.Enqueue(line.Trim());
        }

        Material? material;
        while ((material = TryParse(allLines, folderPath)) != null)
        {
            yield return material;
        }
    }

    private static Material? TryParse(Queue<string> allLines, string folderPath)
    {
        string? firstLine;
        do
        {
            if (allLines.Count == 0)
            {
                return null;
            }

            firstLine = allLines.Dequeue();
        } while (!firstLine.StartsWith("newmtl"));
        string name = SkipFirstTokenSeparatedBySpace(firstLine);

        var lines = new Dictionary<string, string>();
        while (true)
        {
            if (allLines.Count == 0)
            {
                break;
            }

            string line = allLines.Peek();
            //Skip empty lines
            if (line.Length == 0)
            {
                allLines.Dequeue();
                continue;
            }

            int commentStartIndex = line.IndexOf('#');
            //Skip line if it starts with a comment
            if (commentStartIndex == 0)
            {
                allLines.Dequeue();
                continue;
            }

            lines.Add(GetFirstTokenSeparatedBySpace(line), SkipFirstTokenSeparatedBySpace(line));

            if (line.StartsWith("newmtl"))
            {
                break;
            }

            allLines.Dequeue();
        }

        if (lines.Count == 0)
        {
            return null;
        }

        return new Material(name,
                            Ambient.Parse(lines, folderPath),
                            Diffusion.Parse(lines, folderPath),
                            Specular.Parse(lines, folderPath),
                            Opacity.Parse(lines, folderPath),
                            Deformity.Parse(lines, folderPath));
    }

    private static string GetFirstTokenSeparatedBySpace(string text)
    {
        int firstSpaceIndex = text.IndexOf(' ');
        if (firstSpaceIndex == -1)
        {
            throw new InvalidOperationException();
        }

        return text[..firstSpaceIndex];
    }

    private static string SkipFirstTokenSeparatedBySpace(string text)
    {
        int firstSpaceIndex = text.IndexOf(' ');
        if (firstSpaceIndex == -1)
        {
            throw new InvalidOperationException();
        }

        return text[(firstSpaceIndex + 1)..];
    }
}
