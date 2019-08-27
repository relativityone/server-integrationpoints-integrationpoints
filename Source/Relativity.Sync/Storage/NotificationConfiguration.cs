using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class NotificationConfiguration : INotificationConfiguration
	{
		private IEnumerable<string> _emailRecipients;

		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");

		public NotificationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);

		public IEnumerable<string> EmailRecipients => _emailRecipients ?? (_emailRecipients = (_cache.GetFieldValue<string>(EmailNotificationRecipientsGuid) ?? string.Empty)
															.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));

		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;

		public string JobName => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).Name;

		public bool SendEmails => EmailRecipients.Any();

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public string SourceWorkspaceTag => _cache.GetFieldValue<string>(SourceWorkspaceTagNameGuid);

		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
	}
}