using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Storage
{
	internal sealed class NotificationConfiguration : INotificationConfiguration
	{
		private IEnumerable<string> _emailRecipients;

		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;
		private readonly Lazy<string> _jobNameLazy;

		public NotificationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, ISyncServiceManager servicesManager)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
			
			_jobNameLazy = new Lazy<string>(() =>
			{
				using (var objectManager = servicesManager.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
				{
					return objectManager.GetObjectNameAsync(syncJobParameters.WorkspaceId,
							_cache.GetFieldValue(x => x.JobHistoryId), 
							_cache.GetFieldValue(x => x.JobHistoryType))
						.GetAwaiter().GetResult();
				}
			});
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);


		public int JobHistoryArtifactId => _cache.GetFieldValue(x => x.JobHistoryId);

		public bool SendEmails => GetEmailRecipients().Any();

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;

		public IEnumerable<string> GetEmailRecipients() => _emailRecipients ?? (_emailRecipients = (_cache.GetFieldValue<string>(x => x.EmailNotificationRecipients) ?? string.Empty)
			                                              .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));

		public string GetJobName() => _jobNameLazy.Value;

		public string GetSourceWorkspaceTag() => _cache.GetFieldValue<string>(x => x.SourceWorkspaceTagName);
	}
}