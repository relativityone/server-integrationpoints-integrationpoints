namespace Relativity.Sync.SyncConfiguration.Options
{
	/// <summary>
	/// Represents saved search creation options.
	/// </summary>
	public class CreateSavedSearchOptions
	{
		/// <summary>
		/// Determines whether to create saved search in destination workspace.
		/// </summary>
		public bool CreateSavedSearchInDestination { get; }

		/// <summary>
		/// Creates new instance of <see cref="CreateSavedSearchOptions"/> class.
		/// </summary>
		/// <param name="createSavedSearchInDestination">Determines whether to create saved search in destination workspace.</param>
		public CreateSavedSearchOptions(bool createSavedSearchInDestination)
		{
			CreateSavedSearchInDestination = createSavedSearchInDestination;
		}
	}
}
