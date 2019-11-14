using System;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.Sync.Configuration;

namespace kCura.IntegrationPoints.RelativitySync
{
	internal sealed class SyncConfiguration : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IUserContextConfiguration
	{
		public SyncConfiguration(int submittedBy, ImportSettings destinationConfiguration)
		{
			DataDestinationArtifactId = destinationConfiguration.DestinationFolderArtifactId;
			ExecutingUserId = submittedBy;
		}

		public int DataDestinationArtifactId { get; set; }

		public int ExecutingUserId { get; }

		// Currently unused properties
		public string DataDestinationName => string.Empty;
		public bool IsDataDestinationArtifactIdSet => false;
	}
}