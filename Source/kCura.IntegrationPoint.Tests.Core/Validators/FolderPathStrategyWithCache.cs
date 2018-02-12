using System.Collections.Generic;
using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	public abstract class FolderPathStrategyWithCache: IFolderPathStrategy
	{
		private Dictionary<int, string> _folderCache;

		protected FolderPathStrategyWithCache()
		{ 
			_folderCache = new Dictionary<int, string>();
		}

		public string GetFolderPath(Document document)
		{
			string folderPath;
			if (_folderCache.TryGetValue(document.ParentArtifact.ArtifactID, out folderPath))
			{
				return folderPath;
			}

			folderPath = GetFolderPathInternal(document);
			_folderCache[document.ParentArtifact.ArtifactID] = folderPath;

			return folderPath;
		}

		protected abstract string GetFolderPathInternal(Document document);
	}
}