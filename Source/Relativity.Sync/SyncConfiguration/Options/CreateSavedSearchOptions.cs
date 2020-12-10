namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// 
	/// </summary>
	public class CreateSavedSearchOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public bool CreateSavedSearchInDestination { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="createSavedSearchInDestination"></param>
		public CreateSavedSearchOptions(bool createSavedSearchInDestination)
		{
			CreateSavedSearchInDestination = createSavedSearchInDestination;
		}
	}
}
