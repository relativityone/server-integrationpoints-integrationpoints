using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using SystemInterface;
using SystemInterface.IO;
using SystemInterface.Security.AccessControl;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare
{
    public class FakeDirectory : IDirectory
    {
        public IEnumerable<string> EnumerateFiles(string path)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
        {
            throw new System.NotImplementedException();
        }

        public IDirectoryInfo CreateDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public IDirectoryInfo CreateDirectory(string path, IDirectorySecurity directorySecurity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(string path, bool recursive)
        {
            throw new System.NotImplementedException();
        }

        public bool Exists(string path)
        {
            return true;
        }

        public IDirectorySecurity GetAccessControl(string path)
        {
            throw new System.NotImplementedException();
        }

        public IDirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
        {
            throw new System.NotImplementedException();
        }

        public IDateTime GetCreationTime(string path)
        {
            throw new System.NotImplementedException();
        }

        public IDateTime GetCreationTimeUtc(string path)
        {
            throw new System.NotImplementedException();
        }

        public string GetCurrentDirectory()
        {
            throw new System.NotImplementedException();
        }

        public string[] GetDirectories(string path)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetDirectories(string path, string searchPattern)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            throw new System.NotImplementedException();
        }

        public string GetDirectoryRoot(string path)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFiles(string path)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFileSystemEntries(string path)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetFileSystemEntries(string path, string searchPattern)
        {
            throw new System.NotImplementedException();
        }

        public IDateTime GetLastAccessTime(string path)
        {
            throw new System.NotImplementedException();
        }

        public IDateTime GetLastAccessTimeUtc(string path)
        {
            throw new System.NotImplementedException();
        }

        public IDateTime GetLastWriteTime(string path)
        {
            throw new System.NotImplementedException();
        }

        public IDateTime GetLastWriteTimeUtc(string path)
        {
            throw new System.NotImplementedException();
        }

        public string[] GetLogicalDrives()
        {
            throw new System.NotImplementedException();
        }

        public IDirectoryInfo GetParent(string path)
        {
            throw new System.NotImplementedException();
        }

        public void Move(string sourceDirName, string destDirName)
        {
            throw new System.NotImplementedException();
        }

        public void SetAccessControl(string path, IDirectorySecurity directorySecurity)
        {
            throw new System.NotImplementedException();
        }

        public void SetCreationTime(string path, IDateTime creationTime)
        {
            throw new System.NotImplementedException();
        }

        public void SetCreationTimeUtc(string path, IDateTime creationTimeUtc)
        {
            throw new System.NotImplementedException();
        }

        public void SetCurrentDirectory(string path)
        {
            throw new System.NotImplementedException();
        }

        public void SetLastAccessTime(string path, IDateTime lastAccessTime)
        {
            throw new System.NotImplementedException();
        }

        public void SetLastAccessTimeUtc(string path, IDateTime lastAccessTimeUtc)
        {
            throw new System.NotImplementedException();
        }

        public void SetLastWriteTime(string path, IDateTime lastWriteTime)
        {
            throw new System.NotImplementedException();
        }

        public void SetLastWriteTimeUtc(string path, IDateTime lastWriteTimeUtc)
        {
            throw new System.NotImplementedException();
        }
    }
}
