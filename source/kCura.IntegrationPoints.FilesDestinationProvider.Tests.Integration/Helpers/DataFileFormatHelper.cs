using System.IO;
using System.Linq;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers
{
    internal class DataFileFormatHelper
    {
        public static FileInfo GetFileInFormat(string fileFormatExtension, DirectoryInfo directory)
        {
            var files = directory.EnumerateFiles(fileFormatExtension, SearchOption.TopDirectoryOnly).ToList();
            if (files.Count != 1)
            {
                throw new FileNotFoundException("File in given format not found or found more than one file");
            }
            return files.First();
        }

        public static bool FileStartWith(string firstLineStartsWith, FileInfo file)
        {
            using (var reader = new StreamReader(file.FullName))
            {
                var fileFirstLine = reader.ReadLine();
                return fileFirstLine != null && fileFirstLine.StartsWith(firstLineStartsWith);
            }
        }
    }
}