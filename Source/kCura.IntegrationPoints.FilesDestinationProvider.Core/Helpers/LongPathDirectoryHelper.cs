using Relativity.DataExchange.Io;
using ZetaLongPaths;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers
{
    public class LongPathDirectoryHelper : IDirectory
    {
        private readonly object _lockObject = new object();

        public string Combine(string path1, string path2)
        {
            return ZlpPathHelper.Combine(path1, path2);
        }

        public void CreateDirectory(string path)
        {
            lock (_lockObject)
            {
                if (!Exists(path))
                {
                    ZlpIOHelper.CreateDirectory(path);
                }
            }
        }

        public void Delete(string path, bool recursive)
        {
            lock (_lockObject)
            {
                if (Exists(path))
                {
                    ZlpIOHelper.DeleteDirectory(path, recursive);
                }
            }
        }

        public void Delete(string path)
        {
            // Required by interface but not used by this API.
            throw new System.NotImplementedException();
        }

        public void DeleteIfExists(string path, bool recursive, bool throwOnExistsCheck)
        {
            // Note: this method is only implemented to enhance backwards compatibility with IDirectory (e.g. the export code never calls it as of today).
            if (Exists(path, throwOnExistsCheck))
            {
                Delete(path, recursive);
            }
        }

        public bool Exists(string path)
        {
            return ZlpIOHelper.DirectoryExists(path);
        }

        public bool Exists(string path, bool throwOnExistsCheck)
        {
            // Note: this method in only implemented to enhance backwards compatibility with IDirectory (e.g. the export code never calls it as of today).
            if (!throwOnExistsCheck)
            {
                return ZlpIOHelper.DirectoryExists(path);
            }

            // <see cref="Relativity.DataExchange.Io.DirectoryWrap"/> for more details.
            return ZlpIOHelper.GetFileCreationTime(path) != new System.DateTime(1601, 1, 1);
        }

        public IDirectoryInfo GetParent(string path)
        {
            // Required by interface but not used by this API.
            throw new System.NotImplementedException();
        }
    }
}
