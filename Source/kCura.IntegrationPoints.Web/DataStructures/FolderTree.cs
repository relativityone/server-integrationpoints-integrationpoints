using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace kCura.IntegrationPoints.Web.DataStructures
{
    public class FolderTree
    {
        private readonly string _pathSeparatorString = @"\";
        private readonly char _pathSeparatorChar = '\\';

        private readonly FolderTreeCollection _root;
        private int _count;

        public int FolderCount
        {
            get { return _count; }
        }

        public FolderTree()
        {
            _root = new FolderTreeCollection();
        }

        public void AddEntry(string folderPath)
        {
            string sanitizedPath = SanitizePath(folderPath);
            _count += _root.AddEntry(sanitizedPath, 0);
        }

        private string SanitizePath(string path)
        {
            string trimmedPath = path.Trim(_pathSeparatorChar);
            Regex regex = new Regex("(\\\\){2,}");
            string sanitizedPath = regex.Replace(trimmedPath, _pathSeparatorString);
            return sanitizedPath;
        }
    }

    #region FolderTreeCollection
    internal class FolderTreeCollection : Dictionary<string, Folder>
    {
        private readonly string _pathSeparator = @"\";

        public int AddEntry(string folderPath, int begIndex)
        {
            int count = 0;

            if (begIndex < folderPath.Length)
            {
                int endIndex = folderPath.IndexOf(_pathSeparator, begIndex, StringComparison.Ordinal);
                if (endIndex == -1)
                {
                    endIndex = folderPath.Length;
                }

                string folderName = folderPath.Substring(begIndex, endIndex - begIndex).ToLowerInvariant();
                if (!string.IsNullOrEmpty(folderName))
                {
                    Folder folder;

                    if (ContainsKey(folderName))
                    {
                        folder = this[folderName];
                    }
                    else
                    {
                        count++;
                        folder = new Folder { Name = folderName };
                        Add(folderName, folder);
                    }
                    count += folder.Children.AddEntry(folderPath, endIndex + 1);
                }
            }
            return count;
        }
    }
    #endregion

    #region Folder
    internal class Folder
    {
        public string Name { get; set; }
        public FolderTreeCollection Children { get; set; }

        public Folder()
        {
            Children = new FolderTreeCollection();
        }
    }
    #endregion
}