using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISyncConfigurationRootBuilder<out T> : ISyncConfigurationRootBuilder
		where T : ISyncConfigurationRootBuilder
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		T OverwriteMode(OverwriteOptions options);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		T EmailNotifications(EmailNotificationsOptions options);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		T CreateSavedSearch(CreateSavedSearchOptions options);
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		T IsRetry(RetryOptions options);
	}

	/// <summary>
	/// 
	/// </summary>
	public interface ISyncConfigurationRootBuilder
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		int Build();
	}
}
