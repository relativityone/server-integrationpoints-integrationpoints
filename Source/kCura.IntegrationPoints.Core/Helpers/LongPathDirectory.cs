using System;
using System.IO;
using System.Security.AccessControl;
using SystemInterface;
using SystemInterface.IO;
using SystemInterface.Security.AccessControl;

namespace kCura.IntegrationPoints.Core.Helpers
{
	public class LongPathDirectory : IDirectory
	{
		public IDirectoryInfo CreateDirectory(string path)
		{
			throw new NotImplementedException();
		}

		public IDirectoryInfo CreateDirectory(string path, IDirectorySecurity directorySecurity)
		{
			throw new NotImplementedException();
		}

		public void Delete(string path)
		{
			throw new NotImplementedException();
		}

		public void Delete(string path, bool recursive)
		{
			throw new NotImplementedException();
		}

		public bool Exists(string path)
		{
			return LongPath.LongPath.DirectoryExists(path);
		}

		public IDirectorySecurity GetAccessControl(string path)
		{
			throw new NotImplementedException();
		}

		public IDirectorySecurity GetAccessControl(string path, AccessControlSections includeSections)
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

		public string GetCurrentDirectory()
		{
			throw new NotImplementedException();
		}

		public string[] GetDirectories(string path)
		{
			return LongPath.LongPath.GetDirectories(path);
		}

		public string[] GetDirectories(string path, string searchPattern)
		{
			throw new NotImplementedException();
		}

		public string GetDirectoryRoot(string path)
		{
			throw new NotImplementedException();
		}

		public string[] GetFiles(string path)
		{
            //TODO: fix to work with long paths
            return Directory.GetFiles(path);
		}

		public string[] GetFiles(string path, string searchPattern)
		{
			throw new NotImplementedException();
		}

		public string[] GetFileSystemEntries(string path)
		{
			throw new NotImplementedException();
		}

		public string[] GetFileSystemEntries(string path, string searchPattern)
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

		public string[] GetLogicalDrives()
		{
			throw new NotImplementedException();
		}

		public IDirectoryInfo GetParent(string path)
		{
			throw new NotImplementedException();
		}

		public void Move(string sourceDirName, string destDirName)
		{
			throw new NotImplementedException();
		}

		public void SetAccessControl(string path, IDirectorySecurity directorySecurity)
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

		public void SetCurrentDirectory(string path)
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

		public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		public string[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

	}
}
