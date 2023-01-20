using System;
using System.IO;

namespace Relativity.Sync.Extensions
{
    internal static class PathExtensions
    {
        /// <summary>
        /// Creates a relative path from one file or folder to another. - https://stackoverflow.com/a/340454
        /// </summary>
        /// <param name="relativeTo">Contains the directory that defines the start of the relative path.</param>
        /// <param name="path">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string MakeRelativeTo(this string path, string relativeTo)
        {
            if (string.IsNullOrEmpty(relativeTo))
            {
                throw new ArgumentNullException(nameof(relativeTo));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            Uri fromUri = new Uri(relativeTo);
            Uri toUri = new Uri(path);

            if (fromUri.Scheme != toUri.Scheme)
            {
                return path;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string CreateFileWithRecursiveDirectories(string file)
        {
            var longTextFile = new FileInfo(file);

            longTextFile.Directory.Create();

            longTextFile.Create();

            return longTextFile.FullName;
        }
    }
}
