using System;

namespace kCura.IntegrationPoint.FilesDestinationProvider.Core.Files
{
	public interface IFileRepository : IDisposable
	{
		void Create(string filePath);
		void Write(string line);
	}
}
