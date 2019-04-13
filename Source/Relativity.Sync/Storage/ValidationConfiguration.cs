using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;

namespace Relativity.Sync.Storage
{
	internal sealed class ValidationConfiguration : IValidationConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly IFieldMappings _fieldMappings;

		private static readonly Guid DestinationFolderStructureBehaviorGuid = new Guid("A1593105-BD99-4A15-A51A-3AA8D4195908");
		private static readonly Guid DataDestinationArtifactIdGuid = new Guid("0E9D7B8E-4643-41CC-9B07-3A66C98248A1");
		private static readonly Guid DataSourceArtifactIdGuid = new Guid("6D8631F9-0EA1-4EB9-B7B2-C552F43959D0");
		private static readonly Guid DestinationWorkspaceArtifactIdGuid = new Guid("15B88438-6CF7-47AB-B630-424633159C69");
		private static readonly Guid EmailNotificationRecipientsGuid = new Guid("4F03914D-9E86-4B72-B75C-EE48FEEBB583");
		private static readonly Guid FieldOverlayBehaviorGuid = new Guid("34ECB263-1370-4D6C-AC11-558447504EC4");
		private static readonly Guid FolderPathSourceFieldArtifactIdGuid = new Guid("BF5F07A3-6349-47EE-9618-1DD32C9FD998");
		private static readonly Guid ImportOverwriteModeGuid = new Guid("1914D2A3-A1FF-480B-81DC-7A2AA563047A");
		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");

		public ValidationConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_fieldMappings = fieldMappings;
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
		}

		public int SourceWorkspaceArtifactId { get; }
		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue<int>(DestinationWorkspaceArtifactIdGuid);
		public string JobName => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).Name;
		public string NotificationEmails => _cache.GetFieldValue<string>(EmailNotificationRecipientsGuid);
		public int SavedSearchArtifactId => _cache.GetFieldValue<int>(DataSourceArtifactIdGuid);
		public int DestinationFolderArtifactId => _cache.GetFieldValue<int>(DataDestinationArtifactIdGuid);
		public List<FieldMap> FieldMappings => _fieldMappings.GetFieldMappings();
		public int FolderPathSourceFieldArtifactId => _cache.GetFieldValue<int>(FolderPathSourceFieldArtifactIdGuid);
		public ImportOverwriteMode ImportOverwriteMode => (ImportOverwriteMode) (Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue<string>(ImportOverwriteModeGuid)));
		public FieldOverlayBehavior FieldOverlayBehavior => (FieldOverlayBehavior) Enum.Parse(typeof(FieldOverlayBehavior), _cache.GetFieldValue<string>(FieldOverlayBehaviorGuid));
		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior => 
			(DestinationFolderStructureBehavior)(Enum.Parse(typeof(DestinationFolderStructureBehavior), _cache.GetFieldValue<string>(DestinationFolderStructureBehaviorGuid)));
	}
}