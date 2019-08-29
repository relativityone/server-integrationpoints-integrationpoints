using System;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class SyncConfiguration : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, IUserContextConfiguration
	{
		public SyncConfiguration(int submittedBy, ImportSettings destinationConfiguration, ImportSettingsDto importSettings)
		{
			DataDestinationArtifactId = destinationConfiguration.DestinationFolderArtifactId;
			ImportSettings = importSettings;
			ExecutingUserId = submittedBy;
		}

		public int DataDestinationArtifactId { get; set; }

		public int ExecutingUserId { get; }

		public ImportSettingsDto ImportSettings { get; }

		// Currently unused properties
		public string DataDestinationName => string.Empty;
		public Guid ExportRunId => throw new InvalidOperationException();
		public bool IsDataDestinationArtifactIdSet => false;
		public int TotalRecordsCount { get; private set; }
	}
}