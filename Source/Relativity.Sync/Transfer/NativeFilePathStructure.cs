using System;
using System.Linq;

namespace Relativity.Sync.Transfer
{
    internal sealed class NativeFilePathStructure
    {
        public string ServerPath { get; }
        public string FullDirectoryPath { get; }
        public string FileName { get; }

        public NativeFilePathStructure(string fullFilePath)
        {
            string[] allPathComponents = fullFilePath.Split(new char[] { @"\"[0], "/"[0] });

            if (allPathComponents.Length < 5)
            {
                throw new ArgumentException($"Path structure has invalid format");
            }

            string[] serverPathComponents = allPathComponents.Take(4).ToArray();
            string[] directoryPathComponents = allPathComponents.Take(allPathComponents.Length - 1).ToArray();

            ServerPath = string.Join(@"\", serverPathComponents);
            FullDirectoryPath = string.Join(@"\", directoryPathComponents);
            FileName = allPathComponents.Last();
        }
    }
}
