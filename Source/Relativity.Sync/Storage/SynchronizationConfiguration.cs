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


		public SynchronizationConfiguration(IConfiguration cache, SyncJobParameters syncJobParameters, ISerializer serializer, ISyncLog syncLog)
		{
			_cache = cache;
			_syncJobParameters = syncJobParameters;
			_serializer = serializer;
			_syncLog = syncLog;
		}

		public char MultiValueDelimiter => (char) _ASCII_RECORD_SEPARATOR;
		public char NestedValueDelimiter => (char) _ASCII_GROUP_SEPARATOR;
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);
		public int DestinationFolderArtifactId => _cache.GetFieldValue(x => x.DataDestinationArtifactId);
		public int DestinationWorkspaceTagArtifactId => _cache.GetFieldValue<int>(x => x.DestinationWorkspaceTagArtifactId);

		public Guid ExportRunId
		{
			get
			{
				Guid? snapshotId = _cache.GetFieldValue(x => x.SnapshotId);
				if (snapshotId == Guid.Empty)
				{
					snapshotId = null;
				}

				return snapshotId ?? throw new ArgumentException($"Run ID needs to be valid GUID, but null found.");
			}
		}

		public int JobHistoryArtifactId => _cache.GetFieldValue(x => x.JobHistoryId);
		public int SourceJobTagArtifactId => _cache.GetFieldValue(x => x.SourceJobTagArtifactId);
		public int SourceWorkspaceArtifactId => _syncJobParameters.WorkspaceId;
		public int SourceWorkspaceTagArtifactId => _cache.GetFieldValue(x => x.SourceWorkspaceTagArtifactId);
		public int SyncConfigurationArtifactId => _syncJobParameters.SyncConfigurationArtifactId;
		public bool MoveExistingDocuments => _cache.GetFieldValue(x => x.MoveExistingDocuments);

		public int DataSourceArtifactId => _cache.GetFieldValue(x => x.DataSourceArtifactId);
		public int RdoArtifactTypeId => _cache.GetFieldValue(x => x.RdoArtifactTypeId);

		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>
			(DestinationFolderStructureBehavior)(Enum.Parse(typeof(DestinationFolderStructureBehavior), _cache.GetFieldValue(x => x.DestinationFolderStructureBehavior)));
		public ImportOverwriteMode ImportOverwriteMode => (ImportOverwriteMode)(Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue(x => x.ImportOverwriteMode)));
		public FieldOverlayBehavior FieldOverlayBehavior => _cache.GetFieldValue(x => x.FieldOverlayBehavior).GetEnumFromDescription<FieldOverlayBehavior>();
		public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue(x => x.NativesBehavior).GetEnumFromDescription<ImportNativeFileCopyMode>();

		public bool ImageImport => _cache.GetFieldValue(x => x.ImageImport);
		public ImportImageFileCopyMode ImportImageFileCopyMode => _cache.GetFieldValue(x => x.ImageFileCopyMode).GetEnumFromDescription<ImportImageFileCopyMode>();

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