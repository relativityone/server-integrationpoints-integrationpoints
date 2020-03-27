using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.FileExtensionProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerator.SizeCalculator;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerator
{
	public class FileGenerator : IFileGenerator
	{
		private readonly IFileSizeCalculatorStrategy _fileSizeCalculatorStrategy;
		private readonly IFileExtensionProvider _fileExtensionProvider;
		private readonly IFileContentProvider _fileContentProvider;
		private readonly DirectoryInfo _destinationDirectory;

		public FileGenerator(IFileSizeCalculatorStrategy fileSizeCalculatorStrategy, IFileExtensionProvider fileExtensionProvider, IFileContentProvider fileContentProvider, DirectoryInfo destinationDirectory)
		{
			_fileSizeCalculatorStrategy = fileSizeCalculatorStrategy;
			_fileExtensionProvider = fileExtensionProvider;
			_fileContentProvider = fileContentProvider;
			_destinationDirectory = destinationDirectory;
		}

		public Task<IEnumerable<FileInfo>> GenerateAsync(int filesCount, long totalSizeInMB)
		{
			if (filesCount == 0)
			{
				return Task.FromResult(Enumerable.Empty<FileInfo>());
			}

			if (!_destinationDirectory.Exists)
			{
				_destinationDirectory.Create();
			}

			List<FileInfo> files = new List<FileInfo>(filesCount);
			IEnumerable<long> fileSizes = _fileSizeCalculatorStrategy.GetSizesInBytes(filesCount, totalSizeInMB);

			foreach (long size in fileSizes)
			{
				string randomName = Guid.NewGuid().ToString();
				string extension = _fileExtensionProvider.GetFileExtension();
				string fileName = $"{randomName}.{extension}";
				FileInfo file = new FileInfo(Path.Combine(_destinationDirectory.FullName, fileName));
				byte[] data = _fileContentProvider.GetContent(size);
				File.WriteAllBytes(file.FullName, data);

				files.Add(file);
			}

			return Task.FromResult(files.AsEnumerable());
		}
	}
}