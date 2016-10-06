using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class DirectoryTreeCreator<TTreeItem> : IDirectoryTreeCreator<TTreeItem> where TTreeItem : JsTreeItemDTO, new()
	{
		#region Fields

		private readonly IDirectory _directory;

		#endregion //Fields

		#region Constructors

		public DirectoryTreeCreator(IDirectory directory)
		{
			_directory = directory;
		}

		#endregion Constructors

		public List<TTreeItem> GetChildren(string path , bool isRoot)
		{
			if (!CanAccessFolder(path, isRoot))
				return new List<TTreeItem>();
			var subItems = GetSubItems(path);
			return isRoot ? GetRoot(path, subItems) : subItems;
		}

		private static List<TTreeItem> GetRoot(string path, IEnumerable<TTreeItem> subItems)
		{
			const string jstreeRootFolder = "jstree-root-folder";
			var root = new TTreeItem
			{
				Text = path,
				Id = path,
				Icon = jstreeRootFolder,
				Children = new List<JsTreeItemDTO>(subItems)
			};
			return new List<TTreeItem>() {root};
		}

		protected virtual bool CanAccessFolder(string path, bool isRoot)
		{
			ValidateFolder(path, isRoot);
			return true;
		}

		public TTreeItem TraverseTree(string root)
		{
			ValidateFolder(root, true);

			Stack<TTreeItem> directoryItemsToProcessed = new Stack<TTreeItem>();

			TTreeItem rootDirectoryItem = new TTreeItem()
			{
				Id = root,
				Text = root,
                Icon = JsTreeItemIconEnum.Root.GetDescription()
			};

			TTreeItem currDirectoryItem = rootDirectoryItem;
			directoryItemsToProcessed.Push(currDirectoryItem);

			while (directoryItemsToProcessed.Count > 0)
			{
				currDirectoryItem = directoryItemsToProcessed.Pop();

				List<TTreeItem> subItems = GetSubItems(currDirectoryItem);

				currDirectoryItem.Children.AddRange(subItems);

				// Push the subdirectories onto the stack for traversal.
				subItems.ForEach(item => directoryItemsToProcessed.Push(item));
			}
			return rootDirectoryItem;
		}

		protected virtual void ValidateFolder(string path, bool isRoot)
		{
			if (isRoot)
			{
				if (string.IsNullOrEmpty(path))
				{
					throw new ArgumentException($"Argumenent '{nameof(path)}' should not be empty!");
				}
				if (!_directory.Exists(path))
				{
					throw new ArgumentException($"{path} folder does not exist!");
				}
			}
		}

		private List<TTreeItem> GetSubItems(TTreeItem dirItem)
		{
			return GetSubItems(dirItem.Id);
		}

		protected virtual List<TTreeItem> GetSubItems(string path)
		{
			string[] subDirs = new string[0];
			try
			{
				subDirs = _directory.GetDirectories(path);
			}
			// An UnauthorizedAccessException exception will be thrown if we do not have
			// discovery permission on a folder.
			catch (UnauthorizedAccessException)
			{
			}
			return CreateSubItems(subDirs);
		}

		protected virtual List<TTreeItem> CreateSubItems(string[] subDirs)
		{
			return subDirs.Select(subDir =>
				new TTreeItem
				{
					Text = subDir.Substring(subDir.LastIndexOf('\\') + 1),
					Id = subDir
				}).ToList();
		}
	}
}
