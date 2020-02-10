﻿using System;
using System.Collections.Generic;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoints.Domain.Extensions;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Helpers.Implementations
{
	public class DirectoryTreeCreator<TTreeItem> : IDirectoryTreeCreator<TTreeItem> where TTreeItem : JsTreeItemDTO, new()
	{
		private readonly IAPILog _logger;
		private readonly IDirectory _directory;
		private readonly ICryptographyHelper _cryptographyHelper;

		public DirectoryTreeCreator(IDirectory directory, IHelper helper, ICryptographyHelper cryptographyHelper)
		{
			_directory = directory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<DirectoryTreeCreator<TTreeItem>>();
			_cryptographyHelper = cryptographyHelper;
		}
		
		public virtual List<TTreeItem> GetChildren(string path, bool isRoot, bool includeFiles = false)
		{
			if (!CanAccessFolder(path, isRoot))
			{
				return new List<TTreeItem>();
			}

			List<TTreeItem> subItems = GetSubItems(path);
			if (includeFiles)
			{
				subItems.AddRange(GetSubItemsFiles(path));
			}
			return isRoot ? GetRoot(path, subItems) : subItems;
		}

		public TTreeItem TraverseTree(string root, bool includeFiles = false)
		{
			CreateDirectoryIfNotExists(root, true);

			var directoryItemsToProcessed = new Stack<TTreeItem>();

			var rootDirectoryItem = new TTreeItem
			{
				Id = root,
				Text = root,
				Icon = JsTreeItemIconEnum.Root.GetDescription(),
				IsDirectory = true
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

				if (includeFiles)
				{
					List<TTreeItem> subItemsFiles = GetSubItemsFiles(currDirectoryItem);

					currDirectoryItem.Children.AddRange(subItemsFiles);
				}
			}
			return rootDirectoryItem;
		}

		private static List<TTreeItem> GetRoot(string path, IEnumerable<TTreeItem> subItems)
		{
			const string jstreeRootFolder = "jstree-root-folder";
			var root = new TTreeItem
			{
				Text = path,
				Id = path,
				Icon = jstreeRootFolder,
				IsDirectory = true,
				Children = new List<JsTreeItemDTO>(subItems)
			};
			return new List<TTreeItem> {root};
		}

		protected virtual bool CanAccessFolder(string path, bool isRoot)
		{
			CreateDirectoryIfNotExists(path, isRoot);
			return true;
		}

		protected virtual void CreateDirectoryIfNotExists(string path, bool isRoot)
		{
			if (!isRoot)
			{
				return;
			}

			if (string.IsNullOrEmpty(path))
			{
				LogMissingPathArgument();
				throw new ArgumentException($"Argumenent '{nameof(path)}' should not be empty!");
			}

			if (!_directory.Exists(path))
			{
				LogMissingDirectoryCreation(path);
				_directory.CreateDirectory(path);
			}
		}

		private List<TTreeItem> GetSubItems(TTreeItem dirItem)
		{
			return GetSubItems(dirItem.Id);
		}

		protected virtual List<TTreeItem> GetSubItems(string path)
		{
			var subDirs = new string[0];
			try
			{
				subDirs = _directory.GetDirectories(path);
			}
			// An UnauthorizedAccessException exception will be thrown if we do not have
			// discovery permission on a folder.
			catch (UnauthorizedAccessException e)
			{
				LogUnauthorizedAccess(path, e);
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
					IsDirectory = true
				}).ToList();
		}

		private List<TTreeItem> GetSubItemsFiles(TTreeItem dirItem)
		{
			return GetSubItemsFiles(dirItem.Id);
		}

		protected virtual List<TTreeItem> GetSubItemsFiles(string path)
		{
			var subFiles = new string[0];
			try
			{
				subFiles = _directory.GetFiles(path);
			}
			// An UnauthorizedAccessException exception will be thrown if we do not have
			// discovery permission on a folder.
			catch (UnauthorizedAccessException e)
			{
				LogUnauthorizedAccess(path, e);
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
					IsDirectory = false,
					Icon = "jstree-file"
				}).ToList();
		}

		#region Logging
		private void LogMissingDirectoryCreation(string path)
		{
			_logger.LogWarning("Specified folder ({path}) does not exist. Re-creating missing directory.", _cryptographyHelper.CalculateHash(path));
		}

		private void LogMissingPathArgument()
		{
			_logger.LogError("Path argument should not be empty.");
		}

		private void LogUnauthorizedAccess(string path, UnauthorizedAccessException e)
		{
			_logger.LogWarning(e, "Unauthorized access to folder ({Path}) during directory discovery.", _cryptographyHelper.CalculateHash(path));
		}
		
		#endregion
	}
}
