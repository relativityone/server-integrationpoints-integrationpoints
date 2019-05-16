using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class SynchronizationConfiguration : ISynchronizationConfiguration
	{
		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");

		public int SourceWorkspaceArtifactId { get; }
		public int DestinationWorkspaceTagArtifactId { get; }
		public int JobHistoryTagArtifactId { get; }
		public int SyncConfigurationArtifactId { get; }
		public ImportSettingsDto ImportSettings { get; }

		public SynchronizationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, int jobHistoryTagArtifactId)
		{
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
			DestinationWorkspaceTagArtifactId = cache.GetFieldValue<int>(DestinationWorkspaceTagArtifactIdGuid);
			JobHistoryTagArtifactId = jobHistoryTagArtifactId;

			ImportSettings = new ImportSettingsDto
			{

			};
		}
	}
}