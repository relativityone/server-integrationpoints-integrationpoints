﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Common
{
	[ExcludeFromCodeCoverage]
	internal sealed class ConfigurationStub : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration,
		IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, ISnapshotPartitionConfiguration,
		ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, IValidationConfiguration, IUserContextConfiguration, IFieldConfiguration,
		IJobEndMetricsConfiguration, IAutomatedWorkflowTriggerConfiguration
	{
		private IList<FieldMap> _fieldMappings = new List<FieldMap>();

		private const int _ADMIN_ID = 9;
		private const int _ASCII_GROUP_SEPARATOR = 29;
		private const int _ASCII_RECORD_SEPARATOR = 30;

		public string DataDestinationName { get; set; }
		public bool IsDataDestinationArtifactIdSet { get; set; }
		public int DataDestinationArtifactId { get; set; }
		public int DataSourceArtifactId { get; set; }

		public IList<FieldMap> GetFieldMappings() => _fieldMappings;

		public void SetFieldMappings(IList<FieldMap> fieldMappings)
		{
			_fieldMappings = fieldMappings;
		}

		public bool IsSnapshotCreated { get; set; }

		public async Task SetSnapshotDataAsync(Guid runId, int totalRecordsCount)
		{
			await Task.Yield();
			ExportRunId = runId;
			TotalRecordsCount = totalRecordsCount;
			IsSnapshotCreated = true;
		}

		public string SourceWorkspaceTagName { get; set; }
		public bool CreateSavedSearchForTags { get; set; }
		public bool IsSavedSearchArtifactIdSet { get; set; }
		public async Task SetSavedSearchInDestinationArtifactIdAsync(int artifactId)
		{
			SavedSearchArtifactId = artifactId;
			IsSavedSearchArtifactIdSet = true;
			await Task.Yield();
		}

		public string JobName { get; set; }

		public string GetJobName() => JobName;

		public string GetNotificationEmails() => String.Empty;

		public int SourceWorkspaceArtifactId { get; set; }
		public string TriggerName { get; }
		public int SyncConfigurationArtifactId { get; set; }
		public ExecutionResult SynchronizationExecutionResult { get; set; }
		public string TriggerId { get; }
		public string TriggerValue { get; }
		public bool MoveExistingDocuments { get; set; }
		public int RdoArtifactTypeId => (int) ArtifactType.Document;

		public string SourceWorkspaceTag { get; }
		public char MultiValueDelimiter => (char)_ASCII_RECORD_SEPARATOR;
		public char NestedValueDelimiter => (char)_ASCII_GROUP_SEPARATOR;
		public int DestinationWorkspaceArtifactId { get; set; }
		public int SavedSearchArtifactId { get; set; }
		public int DestinationFolderArtifactId { get; set; }
		public int IntegrationPointArtifactId { get; }
		public int SourceProviderArtifactId { get; }

		public string FolderPathSourceFieldName { get; set; }

		public string GetFolderPathSourceFieldName() => FolderPathSourceFieldName;

		public string FileSizeColumn { get; set; }
		public string NativeFilePathSourceFieldName { get; set; }
		public string FileNameColumn { get; set; }
		public string OiFileTypeColumnName { get; set; }
		public string SupportedByViewerColumn { get; set; }
		public ImportOverwriteMode ImportOverwriteMode { get; set; }
		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }
		public ImportNativeFileCopyMode ImportNativeFileCopyMode { get; set; }
		public int IdentityFieldId { get; set; }
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }
		public string SourceJobTagName { get; set; }
		public int SourceJobTagArtifactId { get; set; }
		public int SourceWorkspaceTagArtifactId { get; set; }
		public bool IsDestinationWorkspaceTagArtifactIdSet { get; set; }
		public async Task SetDestinationWorkspaceTagArtifactIdAsync(int artifactId)
		{
			await Task.Yield();
			DestinationWorkspaceTagArtifactId = artifactId;
			IsDestinationWorkspaceTagArtifactIdSet = true;
		}

		public int DestinationWorkspaceTagArtifactId { get; set; }
		public int JobHistoryArtifactId { get; set; }

		public bool IsSourceJobTagSet { get; set; }

		public async Task SetSourceJobTagAsync(int artifactId, string name)
		{
			await Task.Yield();
			SourceJobTagArtifactId = artifactId;
			SourceJobTagName = name;
			IsSourceJobTagSet = true;
		}

		public async Task SetSourceWorkspaceTagAsync(int artifactId, string name)
		{
			await Task.Yield();
			SourceWorkspaceTagArtifactId = artifactId;
			SourceWorkspaceTagName = name;
		}

		public int ExecutingUserId => _ADMIN_ID;
		public bool SendEmails { get; set; }
		public IEnumerable<string> EmailRecipients { get; } = new List<string>();
		public int TotalRecordsCount { get; set; }
		public int BatchSize { get; set; }
		public Guid ExportRunId { get; set; }
		public string WorkflowId { get; }
	}
}