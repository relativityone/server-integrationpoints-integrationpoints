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
			if (CanAccessFolder(path, isRoot))
			{
				return GetSubItems(path);
			}
			return new List<TTreeItem>();
		}

		protected virtual bool CanAccessFolder(string path, bool isRoot)
		{
			ValidateFolder(path, isRoot);
			return true;
		}

		public TTreeItem TraverseTree(string root, bool includeFiles = false)
		{
			ValidateFolder(root, true);

			Stack<TTreeItem> directoryItemsToProcessed = new Stack<TTreeItem>();

			TTreeItem rootDirectoryItem = new TTreeItem()
			{
				Id = root,
				Text = root,
                Icon = JsTreeItemIconEnum.Root.GetDescription(),
                isDirectory = true
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
                //if the optional includeFiles parameter is passed in as true, retrieve files and add to tree structure
                if (includeFiles)
                {
                    List<TTreeItem> subItemsFiles = GetSubItemsFiles(currDirectoryItem);

                    currDirectoryItem.Children.AddRange(subItemsFiles);
                }
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
					Id = subDir,
                    isDirectory = true
				}).ToList();
		}

      //Doing the same for Files
        private List<TTreeItem> GetSubItemsFiles(TTreeItem dirItem)
        {
            return GetSubItemsFiles(dirItem.Id);
        }

        protected virtual List<TTreeItem> GetSubItemsFiles(string path)
        {
            string[] subFiles = new string[0];
            try
            {
                subFiles = _directory.GetFiles(path);
            }
            // An UnauthorizedAccessException exception will be thrown if we do not have
            // discovery permission on a folder.
            catch (UnauthorizedAccessException)
            {
            }
            return CreateSubItemsFiles(subFiles);
        }

        protected virtual List<TTreeItem> CreateSubItemsFiles(string[] subDirs)
        {
            return subDirs.Select(subDir =>
                new TTreeItem
                {
                    Text = subDir.Substring(subDir.LastIndexOf('\\') + 1),
                    Id = subDir,
                    isDirectory = false,
                    Icon = "jstree-file"
                }).ToList();
        }
    }
}
