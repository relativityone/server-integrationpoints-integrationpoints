using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using SystemInterface;
using SystemInterface.IO;

namespace Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare
{
	public class FakeFileInfoFactory : IFileInfoFactory
	{
		private readonly IDictionary<string, IFileInfo> _files = new Dictionary<string, IFileInfo>();

		public IFileInfo Create(FileInfo fileInfo)
		{
			return ReadFileInfo(fileInfo.FullName);
		}

		public IFileInfo Create(string fileName)
		{
			return ReadFileInfo(fileName);
		}

		public void SetupFile(string fullName, long size, DateTime modifiedDate)
		{
			Mock<IDateTime> dateTime = new Mock<IDateTime>();
			dateTime.SetupGet(x => x.DateTimeInstance).Returns(modifiedDate);

			Mock<IFileInfo> fileInfo = new Mock<IFileInfo>();
			fileInfo.SetupGet(x => x.Length).Returns(size);
			fileInfo.SetupGet(x => x.LastWriteTimeUtc)
				.Returns(dateTime.Object);

			string key = GetKey(fullName);

			_files[key] = fileInfo.Object;
		}

		private IFileInfo ReadFileInfo(string fullPath)
		{
			string key = GetKey(fullPath);

			return _files[key];
		}

		private string GetKey(string fullName) => Path.GetFileName(fullName);
	}
}
