using System;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Files
{
	public interface IFileRepository : IDisposable
	{
		void Create(string filePath);
		void Write(string line);
	}
}
