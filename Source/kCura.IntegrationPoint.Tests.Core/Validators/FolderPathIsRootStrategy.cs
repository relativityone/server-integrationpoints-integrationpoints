namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using Relativity.Client.DTOs;

	public class FolderPathIsRootStrategy : FolderPathStrategyWithCache
	{
		private readonly string _folderName;

		public FolderPathIsRootStrategy(string folderName = "")
		{
			_folderName = folderName;
		}

		protected override string GetFolderPathInternal(Document document)
		{
			return _folderName;
		}
	}
}