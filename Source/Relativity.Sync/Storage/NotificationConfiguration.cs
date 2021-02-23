﻿using System;
using System.Collections.Generic;
using System.Linq;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class NotificationConfiguration : INotificationConfiguration
	{
		private IEnumerable<string> _emailRecipients;

		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		public NotificationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);


		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistory).ArtifactID;

		public bool SendEmails => GetEmailRecipients().Any();

		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;

		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;

		public IEnumerable<string> GetEmailRecipients() => _emailRecipients ?? (_emailRecipients = (_cache.GetFieldValue<string>(x => x.EmailNotificationRecipients) ?? string.Empty)
			                                              .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));

		public string GetJobName() => _cache.GetFieldValue<RelativityObjectValue>(JobHistory).Name;

		public string GetSourceWorkspaceTag() => _cache.GetFieldValue<string>(x => x.SourceWorkspaceTagName);
	}
}