using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileExtensionProvider;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerating
{
	public class FileGenerator
	{
		private readonly IFileExtensionProvider _fileExtensionProvider;
		private readonly IFileContentProvider _fileContentProvider;
		private readonly DirectoryInfo _destinationDirectory;

		public FileGenerator(IFileExtensionProvider fileExtensionProvider, IFileContentProvider fileContentProvider, DirectoryInfo destinationDirectory)
		{
			_fileExtensionProvider = fileExtensionProvider;
			_fileContentProvider = fileContentProvider;
			_destinationDirectory = destinationDirectory;
		}

		public Task<FileInfo> GenerateAsync(string name, long sizeInBytes)
		{
			_destinationDirectory.Create();

			string extension = _fileExtensionProvider.GetFileExtension();
			string fileName = $"{name}.{extension}";
			FileInfo file = new FileInfo(Path.Combine(_destinationDirectory.FullName, fileName));
			byte[] data = _fileContentProvider.GetContent(sizeInBytes);

			Console.WriteLine($"Creating file: {fileName}\t\tSize: {sizeInBytes} bytes");
			File.WriteAllBytes(file.FullName, data);
			return Task.FromResult(file);
		}

	}
}