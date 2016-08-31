using System;
using System.Collections.Generic;
using System.IO;
using SystemInterface.IO;
using kCura.IntegrationPoints.Core.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class DirectoryTreeCreator : IDirectoryTreeCreator
	{
		private readonly IDirectory _directory;

		public DirectoryTreeCreator(IDirectory directory)
		{
			_directory = directory;
		}

		public DirectoryTreeItem TraverseTree(string root)
		{
			Stack<DirectoryTreeItem> directoryItemsToProcessed = new Stack<DirectoryTreeItem>();

			if (!_directory.Exists(root))
			{
				throw new ArgumentException($"{root} folder does not exist!");
			}

			DirectoryTreeItem rootDirectoryItem = new DirectoryTreeItem()
			{
				Id = root,
				Text = root
			};

			DirectoryTreeItem currDirectoryItem = rootDirectoryItem;
			directoryItemsToProcessed.Push(currDirectoryItem);

			while (directoryItemsToProcessed.Count > 0)
			{
				currDirectoryItem = directoryItemsToProcessed.Pop();

				string[] subDirs = GetSubItems(currDirectoryItem);
				// Push the subdirectories onto the stack for traversal.
				foreach (string fullPathDir in subDirs)
				{
					var newDirectoryItem = new DirectoryTreeItem
					{
						Text = fullPathDir.Substring(fullPathDir.LastIndexOf('\\') + 1),
						Id = fullPathDir
					};
					directoryItemsToProcessed.Push(newDirectoryItem);
					currDirectoryItem.Children.Add(newDirectoryItem);
				}
			}
			return rootDirectoryItem;
		}

		private string[] GetSubItems(DirectoryTreeItem dirItem)
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
	}
}
