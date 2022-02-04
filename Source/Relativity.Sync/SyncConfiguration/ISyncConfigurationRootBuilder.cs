using System.Threading.Tasks;
using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// Root configuration builder interface, which all specific configuration builders must implement.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISyncConfigurationRootBuilder<out T> : ISyncConfigurationRootBuilder
		where T : ISyncConfigurationRootBuilder
	{
		/// <summary>
		/// Configures the Correlation ID value.
		/// </summary>
		/// <param name="correlationId">New Correlation ID.</param>
		T CorrelationId(string correlationId);

		/// <summary>
		/// Configures overwrite mode.
		/// </summary>
		/// <param name="options">The overwrite options.</param>
		/// <returns></returns>
		T OverwriteMode(OverwriteOptions options);

		/// <summary>
		/// Configures email notification options.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		T EmailNotifications(EmailNotificationsOptions options);

		/// <summary>
		/// Configures saved search creation options.
		/// </summary>
		/// <param name="options">Saved search creation options.</param>
		/// <returns></returns>
		T CreateSavedSearch(CreateSavedSearchOptions options);

		/// <summary>
		/// Determines if this job is a retry job.
		/// </summary>
		/// <param name="options">Retry options.</param>
		/// <returns></returns>
		T IsRetry(RetryOptions options);
		
		/// <summary>
		/// Disables creating JobHistoryError RDOs for item level errors
		/// </summary>
		/// <returns></returns>
		T DisableItemLevelErrorLogging();
	}

	/// <summary>
	/// Root configuration builder interface.
	/// </summary>
	public interface ISyncConfigurationRootBuilder
	{
		/// <summary>
		/// Stores the configuration.
		/// </summary>
		/// <returns>Artifact ID of the saved configuration object.</returns>
		Task<int> SaveAsync();
	}
}
