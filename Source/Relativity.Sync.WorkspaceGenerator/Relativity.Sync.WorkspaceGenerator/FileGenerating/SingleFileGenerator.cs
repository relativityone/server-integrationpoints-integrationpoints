using System;
using System.IO;
using System.Threading.Tasks;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileContentProvider;
using Relativity.Sync.WorkspaceGenerator.FileGenerating.FileExtensionProvider;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerating
{
    public class SingleFileGenerator : IFileGenerator
    {
        private readonly IFileExtensionProvider _fileExtensionProvider;
        private readonly IFileContentProvider _fileContentProvider;
        private readonly long _fileSize;
        private readonly DirectoryInfo _destinationDirectory;

        private FileInfo _generatedFile;

        public SingleFileGenerator(IFileExtensionProvider fileExtensionProvider, IFileContentProvider fileContentProvider, long fileSize, DirectoryInfo destinationDirectory)
        {
            _fileExtensionProvider = fileExtensionProvider;
            _fileContentProvider = fileContentProvider;
            _fileSize = fileSize;
            _destinationDirectory = destinationDirectory;
        }

        public Task<FileInfo> GenerateAsync()
        {
            if (_generatedFile == null || !_generatedFile.Exists)
            {
                _destinationDirectory.Create();

                string extension = _fileExtensionProvider.GetFileExtension();
                string fileName = $"{Guid.NewGuid()}.{extension}";
                string path = Path.Combine(_destinationDirectory.FullName, fileName);

                byte[] data = _fileContentProvider.GetContent(_fileSize);

                Console.WriteLine($"Creating file: {fileName}\t\tSize: {_fileSize} bytes");
                File.WriteAllBytes(path, data);

                _generatedFile = new FileInfo(path);
            }
            
            return Task.FromResult(_generatedFile);
        }
    }
}