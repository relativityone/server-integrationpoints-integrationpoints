using System;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;
using Relativity.Sync.Utils;

namespace Relativity.Sync.Storage
{
	internal sealed class SynchronizationConfiguration : ISynchronizationConfiguration, IDocumentSynchronizationConfiguration, IImageSynchronizationConfiguration
	{
		private const int _ASCII_GROUP_SEPARATOR = 29;
		private const int _ASCII_RECORD_SEPARATOR = 30;

		private readonly IConfiguration _cache;
		private readonly SyncJobParameters _syncJobParameters;
		private readonly ISerializer _serializer;
		private readonly ISyncLog _syncLog;

		private static readonly Guid DestinationWorkspaceTagArtifactIdGuid = new Guid("E2100C10-B53B-43FA-BB1B-51E43DCE8208");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		private static readonly Guid SnapshotIdGuid = new Guid("D1210A1B-C461-46CB-9B73-9D22D05880C5");
		private static readonly Guid SourceJobTagArtifactIdGuid = new Guid("C0A63A29-ABAE-4BF4-A3F4-59E5BD87A33E");
		private static readonly Guid SourceWorkspaceTagArtifactIdGuid = new Guid("FEAB129B-AEEF-4AA4-BC91-9EAE9A4C35F6");
		private static readonly Guid MoveExistingDocumentsGuid = new Guid("26F9BF88-420D-4EFF-914B-C47BA36E10BF");
		private static readonly Guid RdoArtifactTypeIdGuid = new Guid("4DF15F2B-E566-43CE-830D-671BD0786737");

		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid NativesBehaviorGuid = new Guid("D18F0199-7096-4B0C-AB37-4C9A3EA1D3D2");

		private static readonly Guid ImageImportGuid = new Guid("b282bbe4-7b32-41d1-bb50-960a0e483bb5");
		private static readonly Guid ProductionImagePrecedenceGuid = new Guid("421cf05e-bab4-4455-a9ca-fa83d686b5ed");
		private static readonly Guid ImageFileCopyModeGuid = new Guid("bd5dc6d2-faa2-4312-8dc0-4d1b6945dfe1");

		public SynchronizationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, ISerializer serializer, ISyncLog syncLog)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
			_serializer = serializer;
			_syncLog = syncLog;
		}

		public char MultiValueDelimiter => (char) _ASCII_RECORD_SEPARATOR;
		public char NestedValueDelimiter => (char) _ASCII_GROUP_SEPARATOR;
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DestinationWorkspaceArtifactIdGuid);
		public int DestinationFolderArtifactId => _cache.GetFieldValue<int>(SyncConfigurationRdo.DataDestinationArtifactIdGuid);
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

		public int JobHistoryArtifactId => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).ArtifactID;
		public int SourceJobTagArtifactId => _cache.GetFieldValue<int>(SourceJobTagArtifactIdGuid);
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SourceWorkspaceTagArtifactId => _cache.GetFieldValue<int>(SourceWorkspaceTagArtifactIdGuid);
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public bool MoveExistingDocuments => _cache.GetFieldValue<bool>(MoveExistingDocumentsGuid);
		public int RdoArtifactTypeId => _cache.GetFieldValue<int>(RdoArtifactTypeIdGuid);

		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>
			(DestinationFolderStructureBehavior)(Enum.Parse(typeof(DestinationFolderStructureBehavior), _cache.GetFieldValue<string>(SyncConfigurationRdo.DestinationFolderStructureBehaviorGuid)));
		public ImportOverwriteMode ImportOverwriteMode => (ImportOverwriteMode)(Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue<string>(ImportOverwriteModeGuid)));
		public FieldOverlayBehavior FieldOverlayBehavior => _cache.GetFieldValue<string>(SyncConfigurationRdo.FieldOverlayBehaviorGuid).GetEnumFromDescription<FieldOverlayBehavior>();
		public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue<string>(NativesBehaviorGuid).GetEnumFromDescription<ImportNativeFileCopyMode>();

		public bool ImageImport => _cache.GetFieldValue<bool>(ImageImportGuid);
		public ImportImageFileCopyMode ImportImageFileCopyMode => _cache.GetFieldValue<string>(ImageFileCopyModeGuid).GetEnumFromDescription<ImportImageFileCopyMode>();

		// Below settings are set in SynchronizationExecutor.

		public int IdentityFieldId { get; set; }
		public string FolderPathSourceFieldName { get; set; }
		public string FileSizeColumn { get; set; }
		public string NativeFilePathSourceFieldName { get; set; }
		public string ImageFilePathSourceFieldName { get; set; }
		public string FileNameColumn { get; set; }
		public string IdentifierColumn { get; set; }
		public string OiFileTypeColumnName { get; set; }
		public string SupportedByViewerColumn { get; set; }
	}
}