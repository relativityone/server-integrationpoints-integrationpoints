using System;
using System.Collections.Generic;
using System.IO;
using SystemInterface.IO;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class DirectoryTreeCreator : IDirectoryTreeCreator
	{
		private readonly IDirectory _directory;

		public DirectoryTreeCreator(IDirectory directory)
		{
			_directory = directory;
		}

		public JsTreeItemDTO TraverseTree(string root, bool includeFiles = false)

		{
			ValidateFolder(root);

			Stack<JsTreeItemDTO> directoryItemsToProcessed = new Stack<JsTreeItemDTO>();

			JsTreeItemDTO rootDirectoryItem = new JsTreeItemDTO()
			{
				Id = root,
				Text = root,
                isDirectory = true
			};

			JsTreeItemDTO currDirectoryItem = rootDirectoryItem;
			directoryItemsToProcessed.Push(currDirectoryItem);

            while (directoryItemsToProcessed.Count > 0)
            {
                currDirectoryItem = directoryItemsToProcessed.Pop();

                string[] subDirs = GetSubItems(currDirectoryItem);
                // Push the subdirectories onto the stack for traversal.
                foreach (string fullPathDir in subDirs)
                {
                    var newDirectoryItem = new JsTreeItemDTO
                    {
                        Text = fullPathDir.Substring(fullPathDir.LastIndexOf('\\') + 1),
                        Id = fullPathDir,
                        isDirectory = true
                    };
                    directoryItemsToProcessed.Push(newDirectoryItem);
                    currDirectoryItem.Children.Add(newDirectoryItem);
                }
                //if the optional includeFiles parameter is passed in as true, retrieve files and add to tree structure
                if (includeFiles)
                {
                    string[] subFiles = GetSubItemsFiles(currDirectoryItem);
                    // Push the subdirectories onto the stack for traversal.
                    foreach (string fullPathDir in subFiles)
                    {
                        var newDirectoryItem = new JsTreeItemDTO
                        {
                            Text = fullPathDir.Substring(fullPathDir.LastIndexOf('\\') + 1),
                            Id = fullPathDir,
                            isDirectory = false
                        };                        
                        currDirectoryItem.Children.Add(newDirectoryItem);
                    }
                }
            }

			return rootDirectoryItem;
		}

		private void ValidateFolder(string root)
		{
			if (string.IsNullOrEmpty(root))
			{
				throw new ArgumentException($"Argumenent '{nameof(root)}' should not be empty!");
			}
			if (!_directory.Exists(root))
			{
				throw new ArgumentException($"{root} folder does not exist!");
			}
		}

		private string[] GetSubItems(JsTreeItemDTO dirItem)
		{
			string[] subDirs = new string[0];
			try
			{
				subDirs = _directory.GetDirectories(dirItem.Id);
			}
			// An UnauthorizedAccessException exception will be thrown if we do not have
			// discovery permission on a folder.
			catch (UnauthorizedAccessException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
			return subDirs;
		}

        private string[] GetSubItemsFiles(JsTreeItemDTO dirItem)
        {
            string[] subFiles = new string[0];
            try
            {
                subFiles = _directory.GetFiles(dirItem.Id);
            }
            // An UnauthorizedAccessException exception will be thrown if we do not have
            // discovery permission on a folder.
            catch (UnauthorizedAccessException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            return subFiles;
        }
    }
}
