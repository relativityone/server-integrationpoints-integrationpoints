namespace kCura.IntegrationPoint.Tests.Core.Validators
{
	using Relativity.Client.DTOs;

	public class FolderPathIsRootStrategy : FolderPathStrategyWithCache
	{
		protected override string GetFolderPathInternal(Document document)
		{
			return string.Empty;
		}

	}
}