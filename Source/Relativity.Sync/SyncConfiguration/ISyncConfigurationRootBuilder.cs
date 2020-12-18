using System.Threading.Tasks;
using Relativity.Sync.SyncConfiguration.Options;
#pragma warning disable 1591

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
		Task<int> SaveAsync();
	}
}
