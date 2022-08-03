using System.IO;
using SystemInterface.IO;
using SystemWrapper.IO;

namespace kCura.IntegrationPoints.Core.Factories.Implementations
{
    public class StreamFactory : IStreamFactory
    {
        public IFileStream GetFileStream(string filePath)
        {
            return new FileStreamWrap(filePath, FileMode.Open);
        }

        public IMemoryStream GetMemoryStream()
        {
            return new MemoryStreamWrap();
        }
    }
}
