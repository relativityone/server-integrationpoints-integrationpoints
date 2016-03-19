using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace kCura.IntegrationPoints.Web.DataStructures
{
	public class FolderTree
	{
		private readonly TreeView _treeView;

		public FolderTree()
		{
			_treeView = new TreeView { PathSeparator = @"\" };
		}

		public int FolderCount
		{
			get { return _treeView.GetNodeCount(true); }
		}

		/// <summary>
		/// http://stackoverflow.com/questions/1155977/populate-treeview-from-a-list-of-path
		/// </summary>
		/// <param name="path"></param>
		public void AddNode(string path)
		{
			if (String.IsNullOrEmpty(path) || path == @"\")
			{
				return;
			}

			string trimmedFolderPath = path.Trim('\\');
			Regex regex = new Regex("(\\\\){2,}");
			string sanitizedFolderPath = regex.Replace(trimmedFolderPath, @"\");

			TreeNode lastNode = null;
			char pathSeparator = '\\';
			var subPathAggregate = string.Empty;

			foreach (string subPath in sanitizedFolderPath.Split(pathSeparator))
			{
				subPathAggregate += subPath + pathSeparator;
				TreeNode[] nodes = _treeView.Nodes.Find(subPathAggregate, true);
				if (nodes.Length == 0)
				{
					if (lastNode == null)
					{
						lastNode = _treeView.Nodes.Add(subPathAggregate, subPath);
					}
					else
					{
						lastNode = lastNode.Nodes.Add(subPathAggregate, subPath);
					}
				}
				else
				{
					lastNode = nodes[0];
				}
			}
			lastNode = null; // This is the place code was changed
		}
	}
}