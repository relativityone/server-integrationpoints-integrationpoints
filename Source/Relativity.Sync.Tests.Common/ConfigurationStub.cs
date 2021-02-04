using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common.RdoGuidProviderStubs;

namespace Relativity.Sync.Tests.Common
{
	[ExcludeFromCodeCoverage]
	internal sealed class ConfigurationStub : IDataDestinationFinalizationConfiguration, IDataDestinationInitializationConfiguration, IDataSourceSnapshotConfiguration,
		IDestinationWorkspaceObjectTypesCreationConfiguration, IDestinationWorkspaceSavedSearchCreationConfiguration, IDestinationWorkspaceTagsCreationConfiguration, IJobCleanupConfiguration,
		IJobStatusConsolidationConfiguration, INotificationConfiguration, IPermissionsCheckConfiguration, ISnapshotPartitionConfiguration,
		ISourceWorkspaceTagsCreationConfiguration, ISynchronizationConfiguration, IValidationConfiguration, IUserContextConfiguration, IFieldConfiguration, IImageRetrieveConfiguration,
		IJobEndMetricsConfiguration, IAutomatedWorkflowTriggerConfiguration, IRetryDataSourceSnapshotConfiguration, IPipelineSelectorConfiguration,
		IDocumentDataSourceSnapshotConfiguration, IDocumentRetryDataSourceSnapshotConfiguration, IImageDataSourceSnapshotConfiguration, IImageRetryDataSourceSnapshotConfiguration,
		IDocumentSynchronizationConfiguration, IImageSynchronizationConfiguration, IPreValidationConfiguration, IRdoGuidConfiguration
	{
		private IList<FieldMap> _fieldMappings = new List<FieldMap>();
		private string _jobName = String.Empty;
		private string _sourceJobTagName = String.Empty;
		private string _emailNotificationRecipients;

		private const int _ADMIN_ID = 9;
		private const int _ASCII_GROUP_SEPARATOR = 29;
		private const int _ASCII_RECORD_SEPARATOR = 30;

		private readonly IEnumerable<string> _emailRecipients = new List<string>();

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

		public string GetJobName() => _jobName;

		public void SetJobName(string jobName)
		{
			_jobName = jobName;
		}

		public IEnumerable<string> GetEmailRecipients() => _emailRecipients;

		public string GetNotificationEmails() => _emailNotificationRecipients;

		public void SetEmailNotificationRecipients(string emailNotificationRecipients)
		{
			_emailNotificationRecipients = emailNotificationRecipients;
		}

		public int SourceWorkspaceArtifactId { get; set; }

		public string TriggerName { get; }

		public int SyncConfigurationArtifactId { get; set; }

		public ExecutionResult SynchronizationExecutionResult { get; set; }

		public string TriggerId { get; }

		public string TriggerValue { get; }

		public bool MoveExistingDocuments { get; set; }

		public int RdoArtifactTypeId => (int)ArtifactType.Document;

		public string GetSourceWorkspaceTag() => string.Empty;

		public char MultiValueDelimiter => (char)_ASCII_RECORD_SEPARATOR;

		public char NestedValueDelimiter => (char)_ASCII_GROUP_SEPARATOR;

		public int DestinationWorkspaceArtifactId { get; set; }

		public int SavedSearchArtifactId { get; set; }

		public int DestinationFolderArtifactId { get; set; }

		public int SourceProviderArtifactId { get; }

		public string FolderPathSourceFieldName { get; set; }

		public string GetFolderPathSourceFieldName() => FolderPathSourceFieldName;

		public string FileSizeColumn { get; set; }

		public string NativeFilePathSourceFieldName { get; set; }

		public string ImageFilePathSourceFieldName { get; set; }

		public string FileNameColumn { get; set; }

		public string OiFileTypeColumnName { get; set; }

		public string SupportedByViewerColumn { get; set; }

		public ImportOverwriteMode ImportOverwriteMode { get; set; }

		public FieldOverlayBehavior FieldOverlayBehavior { get; set; }

		public ImportNativeFileCopyMode ImportNativeFileCopyMode { get; set; }

		public int IdentityFieldId { get; set; }

		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior { get; set; }

		public bool ImageImport { get; set; }

		public ImportImageFileCopyMode ImportImageFileCopyMode { get; set; }

		public int[] ProductionImagePrecedence { get; set; } = { };

		public string GetSourceJobTagName() => _sourceJobTagName;

		public void SetSourceJobTagName(string sourceJobTagName)
		{
			_sourceJobTagName = sourceJobTagName;
		}

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

		public Task SetSourceJobTagAsync(int artifactId, string name)
		{
			SourceJobTagArtifactId = artifactId;
			_sourceJobTagName = name;
			IsSourceJobTagSet = true;

			return Task.CompletedTask;
		}

		public async Task SetSourceWorkspaceTagAsync(int artifactId, string name)
		{
			await Task.Yield();
			SourceWorkspaceTagArtifactId = artifactId;
			SourceWorkspaceTagName = name;
		}

		public int ExecutingUserId { get; set; } = _ADMIN_ID;

		public bool SendEmails { get; set; }

		public int TotalRecordsCount { get; set; }

		public int BatchSize { get; set; }

		public Guid ExportRunId { get; set; }

		public int? JobHistoryToRetryId { get; set; }

		public bool IncludeOriginalImageIfNotFoundInProductions { get; set; }

		public bool IsImageJob { get; set; }

		public string IdentifierColumn { get; set; }

		public IJobHistoryRdoGuidsProvider JobHistory { get; } = new JobHistoryRdoGuidsProviderStub
		{
			TypeGuid = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9"),
			CompletedItemsFieldGuid = new Guid("70680399-c8ea-4b12-b711-e9ecbc53cb1c"),
			DestinationWorkspaceInformationGuid = new Guid("20a24c4e-55e8-4fc2-abbe-f75c07fad91b"),
			FailedItemsFieldGuid = new Guid("c224104f-c1ca-4caa-9189-657e01d5504e"),
			TotalItemsFieldGuid = new Guid("576189a9-0347-4b20-9369-b16d1ac89b4b")
		};

		public IJobHistoryErrorGuidsProvider JobHistoryError { get; } = new JobHistoryErrorGuidsProviderStub
		{
			TypeGuid = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB"),
			JobHistoryRelationGuid = new Guid("8b747b91-0627-4130-8e53-2931ffc4135f"),
			SourceUniqueIdGuid = new Guid("5519435e-ee82-4820-9546-f1af46121901"),
			ErrorMessagesGuid = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D"),
			ErrorStatusGuid = new Guid("DE1A46D2-D615-427A-B9F2-C10769BC2678"),
			ErrorTypeGuid = new Guid("EEFFA5D3-82E3-46F8-9762-B4053D73F973"),
			NameGuid = new Guid("84E757CC-9DA2-435D-B288-0C21EC589E66"),
			StackTraceGuid = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF"),
			TimeStampGuid = new Guid("B9CBA772-E7C9-493E-B7F8-8D605A6BFE1F"),
			ItemLevelErrorGuid = new Guid("9DDC4914-FEF3-401F-89B7-2967CD76714B"),
			JobLevelErrorGuid = new Guid("FA8BB625-05E6-4BF7-8573-012146BAF19B"),
			ErrorTypes = new JobHistoryErrorStatusGuidProviderStub
			{
				Expired =  new Guid("AF01A8FA-B419-49B1-BD71-25296E221E57"),
				New = new Guid("F881B199-8A67-4D49-B1C1-F9E68658FB5A"),
				InProgress = new Guid("E5EBD98C-C976-4FA2-936F-434E265EA0AA"),
				Retried = new Guid("7D3D393D-384F-434E-9776-F9966550D29A")
			}
		};
	}
}