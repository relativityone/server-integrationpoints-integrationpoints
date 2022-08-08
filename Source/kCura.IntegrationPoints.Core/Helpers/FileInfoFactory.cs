using System.IO;
using SystemInterface.IO;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public class FileInfoFactory : IFileInfoFactory
    {
        public IFileInfo Create(FileInfo fileInfo)
        {
            return new FileInfoWrap(fileInfo);
        }

        public IFileInfo Create(string fileName)
        {
            return new FileInfoWrap(fileName);
        }
    }
}
