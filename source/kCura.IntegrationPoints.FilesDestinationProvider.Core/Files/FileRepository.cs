using System.IO;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Files
{
	public class FileRepository : IFileRepository
	{
		private StreamWriter _fileStream;

		public void Create(string filePath)
		{
			_fileStream = new StreamWriter(filePath);
		}

		public void Write(string line)
		{
			_fileStream.Write(line);
		}

		public void Dispose()
		{
			_fileStream.Dispose();
		}
	}
}
