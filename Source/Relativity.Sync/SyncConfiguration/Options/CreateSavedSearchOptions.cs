#pragma warning disable 1591
namespace Relativity.Sync.SyncConfiguration.Options
{
	public class CreateSavedSearchOptions
	{
		public bool CreateSavedSearchInDestination { get; }

		public CreateSavedSearchOptions(bool createSavedSearchInDestination)
		{
			CreateSavedSearchInDestination = createSavedSearchInDestination;
		}
	}
}
