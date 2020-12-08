using Relativity.Sync.SyncConfiguration.Options;

namespace Relativity.Sync.SyncConfiguration
{
	public interface ISyncConfigurationRootBuilder<out T> : ISyncConfigurationRootBuilder
		where T : ISyncConfigurationRootBuilder
	{
		T OverwriteMode(OverwriteOptions options);
		T EmailNotifications(EmailNotificationsOptions options);
		T CreateSavedSearch(CreateSavedSearchOptions options);
		T IsRetry(RetryOptions options);
	}

	public interface ISyncConfigurationRootBuilder
	{
		int Build();
	}
}
