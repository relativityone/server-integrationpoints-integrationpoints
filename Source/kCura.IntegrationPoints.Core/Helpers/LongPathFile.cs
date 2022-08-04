using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using SystemInterface;
using SystemInterface.IO;
using SystemInterface.Security.AccessControl;

namespace kCura.IntegrationPoints.Core.Helpers
{
    public class LongPathFile : IFile
    {
        public bool Exists(string path)
        {
            return LongPath.LongPath.FileExists(path);
        }

        public void AppendAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }

        public void AppendAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public IStreamWriter AppendText(string path)
        {
            throw new NotImplementedException();
        }

        public void Copy(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }

        public void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public IFileStream Create(string path)
        {
            throw new NotImplementedException();
        }

        public IFileStream Create(string path, int bufferSize)
        {
            throw new NotImplementedException();
        }

        public IFileStream Create(string path, int bufferSize, FileOptions options)
        {
            throw new NotImplementedException();
        }

        public IFileStream Create(string path, int bufferSize, FileOptions options, IFileSecurity fileSecurity)
        {
            throw new NotImplementedException();
        }

        public IStreamWriter CreateText(string path)
        {
            throw new NotImplementedException();
        }

        public void Decrypt(string path)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public void Encrypt(string path)
        {
            throw new NotImplementedException();
        }

        public IFileSecurity GetAccessControl(string path)
        {
            throw new NotImplementedException();
        }

        public IFileSecurity GetAccessControl(string path, AccessControlSections includeSections)
        {
            throw new NotImplementedException();
        }

        public FileAttributes GetAttributes(string path)
        {
            throw new NotImplementedException();
        }

        public IDateTime GetCreationTime(string path)
        {
            throw new NotImplementedException();
        }

        public IDateTime GetCreationTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public IDateTime GetLastAccessTime(string path)
        {
            throw new NotImplementedException();
        }

        public IDateTime GetLastAccessTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public IDateTime GetLastWriteTime(string path)
        {
            throw new NotImplementedException();
        }

        public IDateTime GetLastWriteTimeUtc(string path)
        {
            throw new NotImplementedException();
        }

        public void Move(string sourceFileName, string destFileName)
        {
            throw new NotImplementedException();
        }

        public IFileStream Open(string path, FileMode mode)
        {
            throw new NotImplementedException();
        }

        public IFileStream Open(string path, FileMode mode, FileAccess access)
        {
            throw new NotImplementedException();
        }

        public IFileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public IFileStream OpenRead(string path)
        {
            throw new NotImplementedException();
        }

        public IStreamReader OpenText(string path)
        {
            throw new NotImplementedException();
        }

        public IFileStream OpenWrite(string path)
        {
            throw new NotImplementedException();
        }

        public byte[] ReadAllBytes(string path)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(string path)
        {
            throw new NotImplementedException();
        }

        public string[] ReadAllLines(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path)
        {
            throw new NotImplementedException();
        }

        public string ReadAllText(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
        {
            throw new NotImplementedException();
        }

        public void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            throw new NotImplementedException();
        }

        public void SetAccessControl(string path, IFileSecurity fileSecurity)
        {
            throw new NotImplementedException();
        }

        public void SetAttributes(string path, FileAttributes fileAttributes)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTime(string path, IDateTime creationTime)
        {
            throw new NotImplementedException();
        }

        public void SetCreationTimeUtc(string path, IDateTime creationTimeUtc)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTime(string path, IDateTime lastAccessTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastAccessTimeUtc(string path, IDateTime lastAccessTimeUtc)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTime(string path, IDateTime lastWriteTime)
        {
            throw new NotImplementedException();
        }

        public void SetLastWriteTimeUtc(string path, IDateTime lastWriteTimeUtc)
        {
            throw new NotImplementedException();
        }

        public void WriteAllBytes(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public void WriteAllLines(string path, string[] contents)
        {
            throw new NotImplementedException();
        }

        public void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void WriteAllText(string path, string contents)
        {
            throw new NotImplementedException();
        }

        public void WriteAllText(string path, string contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public void AppendAllLines(string path, IEnumerable<string> contents)
        {
            throw new NotImplementedException();
        }

        public void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ReadLines(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            throw new NotImplementedException();
        }
    }
}
