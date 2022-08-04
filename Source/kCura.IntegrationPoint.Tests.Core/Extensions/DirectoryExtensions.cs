using kCura.Utility;

namespace kCura.IntegrationPoint.Tests.Core.Extensions
{
    public static class DirectoryExtensions
    {
        public static void CreateDirectoryIfNotExist(this Directory directory, string path)
        {
            if (!directory.Exists(path, exceptOnAccessError: false))
            {
                directory.CreateDirectory(path);
            }
        }
    }
}
