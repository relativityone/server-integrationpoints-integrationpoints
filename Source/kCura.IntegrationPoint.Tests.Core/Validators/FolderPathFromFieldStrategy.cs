using kCura.Relativity.Client.DTOs;

namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	public class FolderPathFromFieldStrategy : FolderPathStrategyWithCache
	{
		private readonly string _folderPathfieldName;

		public FolderPathFromFieldStrategy(string folderPathfieldName)
		{
			_folderPathfieldName = folderPathfieldName;
		}

		protected override string GetFolderPathInternal(Document document)
		{
			return document[_folderPathfieldName].Value?.ToString() ?? "";
		}
	}
}