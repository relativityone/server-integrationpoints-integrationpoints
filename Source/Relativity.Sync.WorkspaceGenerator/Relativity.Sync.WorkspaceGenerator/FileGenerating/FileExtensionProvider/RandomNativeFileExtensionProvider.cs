using System;
using System.IO;

namespace Relativity.Sync.WorkspaceGenerator.FileGenerating.FileExtensionProvider
{
    public class RandomNativeFileExtensionProvider : IFileExtensionProvider
    {
        private readonly Random _random;
        private readonly string[] _supportedFileTypes;

        public RandomNativeFileExtensionProvider()
        {
            _random = new Random();
            _supportedFileTypes = File.ReadAllLines("SupportedFileTypes.txt");
        }

        public string GetFileExtension()
        {
            return _supportedFileTypes[_random.Next(0, _supportedFileTypes.Length)];
        }
    }
}