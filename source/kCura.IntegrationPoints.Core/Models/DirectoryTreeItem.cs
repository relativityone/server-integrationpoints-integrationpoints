using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Models
{
	public class DirectoryTreeItem
	{
		public DirectoryTreeItem()
		{
			Children = new List<DirectoryTreeItem>();
		}
		/// <summary>
		/// Folder Name
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Full Path
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Child subfolders
		/// </summary>
		public List<DirectoryTreeItem> Children { get; set; }
	}
}
