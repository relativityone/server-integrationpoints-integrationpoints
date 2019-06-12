using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Storage
{
	internal class SumReporterConfiguration : ISumReporterConfiguration
	{
		private string _workflowId;

		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		public SumReporterConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
		}

		public string WorkflowId
		{
			get
			{
				if (string.IsNullOrEmpty(_workflowId))
				{
					_workflowId = $"{TelemetryConstants.PROVIDER_NAME}_{IntegrationPointArtifactId}_{JobHistoryArtifactId}";
				}
				return _workflowId;
			}
		}

		private int IntegrationPointArtifactId => _syncJobParameters.IntegrationPointArtifactId;
		private int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;
	}
}