using System.IO;
using System.Security.AccessControl;
using SystemInterface;
using SystemInterface.IO;
using SystemInterface.Security.AccessControl;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public class FileInfoWrap : IFileInfo
    {
        private FileInfo _fileInfo;

        public FileInfoWrap(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public FileInfoWrap(string fileName)
        {
            _fileInfo = new FileInfo(fileName);
        }

        public long Length => _fileInfo.Length;

        public IDateTime LastWriteTimeUtc 
        { 
            get => new DateTimeWrap(_fileInfo.LastWriteTimeUtc); 
            set => throw new System.NotImplementedException(); 
        }

        #region Not Implemented

        public FileAttributes Attributes { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IDateTime CreationTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IDateTime CreationTimeUtc { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public IDirectoryInfo Directory => throw new System.NotImplementedException();

        public string DirectoryName => throw new System.NotImplementedException();

        public bool Exists => throw new System.NotImplementedException();

        public string Extension => throw new System.NotImplementedException();

        public FileInfo FileInfoInstance => throw new System.NotImplementedException();

        public string FullName => throw new System.NotImplementedException();

        public bool IsReadOnly { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IDateTime LastAccessTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IDateTime LastAccessTimeUtc { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IDateTime LastWriteTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string Name => throw new System.NotImplementedException();

        public IStreamWriter AppendText()
        {
            throw new System.NotImplementedException();
        }

        public IFileInfo CopyTo(string destFileName)
        {
            throw new System.NotImplementedException();
        }

        public IFileInfo CopyTo(string destFileName, bool overwrite)
        {
            throw new System.NotImplementedException();
        }

        public IFileStream Create()
        {
            throw new System.NotImplementedException();
        }

        public IStreamWriter CreateText()
        {
            throw new System.NotImplementedException();
        }

        public void Decrypt()
        {
            throw new System.NotImplementedException();
        }

        public void Delete()
        {
            throw new System.NotImplementedException();
        }

        public void Encrypt()
        {
            throw new System.NotImplementedException();
        }

        public IFileSecurity GetAccessControl()
        {
            throw new System.NotImplementedException();
        }

        public IFileSecurity GetAccessControl(AccessControlSections includeSections)
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(FileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        public void Initialize(string fileName)
        {
            throw new System.NotImplementedException();
        }

        public void MoveTo(string destFileName)
        {
            throw new System.NotImplementedException();
        }

        public IFileStream Open(FileMode mode)
        {
            throw new System.NotImplementedException();
        }

        public IFileStream Open(FileMode mode, FileAccess access)
        {
            throw new System.NotImplementedException();
        }

        public IFileStream Open(FileMode mode, FileAccess access, FileShare share)
        {
            throw new System.NotImplementedException();
        }

        public IFileStream OpenRead()
        {
            throw new System.NotImplementedException();
        }

        public IStreamReader OpenText()
        {
            throw new System.NotImplementedException();
        }

        public IFileStream OpenWrite()
        {
            throw new System.NotImplementedException();
        }

        public void Refresh()
        {
            throw new System.NotImplementedException();
        }

        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName)
        {
            throw new System.NotImplementedException();
        }

        public IFileInfo Replace(string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            throw new System.NotImplementedException();
        }

        public void SetAccessControl(IFileSecurity fileSecurity)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
