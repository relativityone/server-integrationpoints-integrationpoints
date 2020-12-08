namespace Relativity.Sync.SyncConfiguration.Options
{
	public class CreateSavedSearchOptions
	{
		public bool CreateSavedSearchInDestination { get; set; }

		public CreateSavedSearchOptions(bool createSavedSearchInDestination)
		{
			CreateSavedSearchInDestination = createSavedSearchInDestination;
		}
	}
}
