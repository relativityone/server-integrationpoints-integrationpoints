using SystemInterface.IO;

namespace kCura.IntegrationPoints.Core.Factories
{
    public interface IStreamFactory
    {
        IFileStream GetFileStream(string filePath);
        IMemoryStream GetMemoryStream();
    }
}
