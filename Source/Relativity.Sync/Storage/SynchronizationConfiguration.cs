using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage
{
	internal sealed class SynchronizationConfiguration : ISynchronizationConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;
		private readonly ISyncLog _syncLog;

		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SourceJobTagNameGuid = new Guid("DA0E1931-9460-4A61-9033-A8035697C1A4");
		private static readonly Guid SourceWorkspaceTagNameGuid = new Guid("D828B69E-AAAE-4639-91E2-416E35C163B1");
		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");

		public SynchronizationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, ISyncLog syncLog)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
			_syncLog = syncLog;
		}

		public int DestinationWorkspaceTagArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceTagArtifactIdGuid);
		public Guid ExportRunId
		{
			get
			{
				string runId = _cache.GetFieldValue<string>(SnapshotIdGuid);
				Guid guid;
				if (Guid.TryParse(runId, out guid))
				{
					return guid;
				}

				_syncLog.LogError("Unable to parse export run ID {runId}.", runId);
				throw new ArgumentException($"Run ID needs to be valid GUID, but {runId} found.");
			}
		}
		public ImportSettingsDto ImportSettings => _syncJobParameters.ImportSettings;
		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;
		public string SourceJobTagName => _cache.GetFieldValue<string>(SourceJobTagNameGuid);
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public string SourceWorkspaceTagName => _cache.GetFieldValue<string>(SourceWorkspaceTagNameGuid);
		public int SyncConfigurationArtifactId => _syncJobParameters.JobId;
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>
			(DestinationFolderStructureBehavior)(Enum.Parse(typeof(DestinationFolderStructureBehavior), _cache.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid)));
	}
}