using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;
#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{
	internal abstract class SyncConfigurationRootBuilderBase : ISyncConfigurationRootBuilder
	{
		protected readonly ISyncServiceManager ServicesMgr;
		protected readonly ISerializer Serializer;
		protected readonly ISyncContext SyncContext;
		
		public readonly SyncConfigurationRdo SyncConfiguration;

		protected SyncConfigurationRootBuilderBase(ISyncContext syncContext, ISyncServiceManager servicesMgr, ISerializer serializer)
		{
			SyncContext = syncContext;
			ServicesMgr = servicesMgr;
			Serializer = serializer;

			SyncConfiguration = new SyncConfigurationRdo
			{
				DestinationWorkspaceArtifactId = SyncContext.DestinationWorkspaceId,
				ImportOverwriteMode = ImportOverwriteMode.AppendOnly.GetDescription(),
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings.GetDescription()
			};
		}

		public void OverwriteMode(OverwriteOptions options)
		{
			SyncConfiguration.ImportOverwriteMode = options.OverwriteMode.ToString();
			SyncConfiguration.FieldOverlayBehavior = options.FieldsOverlayBehavior.GetDescription();
		}

		public void EmailNotifications(EmailNotificationsOptions options)
		{
			SyncConfiguration.EmailNotificationRecipients = string.Join(
				";", options.Emails.Select(x => x.Trim()));
		}

		public void CreateSavedSearch(CreateSavedSearchOptions options)
		{
			SyncConfiguration.CreateSavedSearchInDestination = options.CreateSavedSearchInDestination;
		}

		public void IsRetry(RetryOptions options)
		{
			SyncConfiguration.JobHistoryToRetry = options.JobToRetry;
		}

		public async Task<int> SaveAsync()
		{
			await ValidateAsync().ConfigureAwait(false);

			bool exists = await SyncConfigurationRdo.ExistsAsync(SyncContext.SourceWorkspaceId, ServicesMgr)
				.ConfigureAwait(false);
			if (!exists)
			{
				await SyncConfigurationRdo
					.CreateTypeAsync(SyncContext.SourceWorkspaceId, SyncContext.ParentObjectId, ServicesMgr)
					.ConfigureAwait(false);
			}

			return await SyncConfiguration.SaveAsync(SyncContext.SourceWorkspaceId, SyncContext.ParentObjectId, ServicesMgr)
				.ConfigureAwait(false);
		}

		protected abstract Task ValidateAsync();
	}
}
