namespace Rays.GeometryLoaders.Tests;

internal sealed class TemporaryFile : IDisposable
{
    public string FileName { get; }

    public TemporaryFile(string fileContent)
    {
        FileName = Path.GetTempFileName();
        File.WriteAllText(FileName, fileContent);
    }

    public void Dispose()
    {
        File.Delete(FileName);
    }
}