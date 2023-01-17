using System.IO;

namespace Relativity.Sync.Utils
{
    /// <summary>
    /// Simplifies directory-connected operations.
    /// </summary>
    internal class DirectoryHelper
    {
        /// <summary>
        /// Deletes directory with whole content.
        /// </summary>
        /// <param name="directoryPath">Path to the directory.</param>
        public static void DeleteDirectory(string directoryPath)
        {
            var directory = new DirectoryInfo(directoryPath);
            DeleteDirectory(directory);
        }

        /// <summary>
        /// Deletes directory with whole content.
        /// </summary>
        /// <param name="directory">Directory to delete.</param>
        public static void DeleteDirectory(DirectoryInfo directory)
        {
            if (directory != null && Directory.Exists(directory.FullName))
            {
                foreach (DirectoryInfo dirItem in directory.GetDirectories())
                {
                    DeleteDirectory(dirItem);
                }

                foreach (FileInfo fileItem in directory.GetFiles())
                {
                    try
                    {
                        fileItem.Delete();
                    }
                    catch (FileNotFoundException)
                    {
                        // intentionally silence the exception
                    }
                }

                try
                {
                    directory.Delete();
                }
                catch (DirectoryNotFoundException)
                {
                    // intentionally silence the exception
                }
            }
        }
    }
}
