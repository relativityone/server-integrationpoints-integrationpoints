using System;
using System.Linq;

namespace Relativity.Sync.Transfer
{
    internal sealed class NativeFilePathStructure
    {
        public string ServerPath { get; }
        public string FullDirectoryPath { get; }
        public string FileGUID { get; }

        public NativeFilePathStructure(string fullFilePath)
        {
            string[] allPathComponents = fullFilePath.Split(new char[] { @"\"[0], "/"[0] });
            string[] serverPathComponents = allPathComponents.Take(4).ToArray();
            string[] directoryPathComponents = allPathComponents.Take(allPathComponents.Length - 1).ToArray();

            ServerPath = string.Join(@"\", serverPathComponents);
            FullDirectoryPath = string.Join(@"\", directoryPathComponents);
            FileGUID = allPathComponents.Last();
        }
    }
}
