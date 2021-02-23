using System;
using System.Collections.Generic;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage
{
	internal sealed class ValidationConfiguration : IValidationConfiguration
	{
		private readonly IConfiguration _cache;
		private readonly IFieldMappings _fieldMappings;

		private static readonly Guid JobHistoryGuid = new Guid("5D8F7F01-25CF-4246-B2E2-C05882539BB2");
		
		public int SourceWorkspaceArtifactId { get; }

		public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

		public int SavedSearchArtifactId => _cache.GetFieldValue(x => x.DataSourceArtifactId);

		public int DestinationFolderArtifactId => _cache.GetFieldValue(x => x.DataDestinationArtifactId);

		public ImportOverwriteMode ImportOverwriteMode => (ImportOverwriteMode)Enum.Parse(typeof(ImportOverwriteMode), _cache.GetFieldValue(x => x.ImportOverwriteMode));

		public FieldOverlayBehavior FieldOverlayBehavior => _cache.GetFieldValue(x => x.FieldOverlayBehavior).GetEnumFromDescription<FieldOverlayBehavior>();

		public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>
			(DestinationFolderStructureBehavior)Enum.Parse(typeof(DestinationFolderStructureBehavior), _cache.GetFieldValue(x => x.DestinationFolderStructureBehavior));

		public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue(x => x.NativesBehavior).GetEnumFromDescription<ImportNativeFileCopyMode>();

		public ImportImageFileCopyMode ImportImageFileCopyMode => _cache.GetFieldValue(x => x.ImageFileCopyMode).GetEnumFromDescription<ImportImageFileCopyMode>();

		public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

		public ValidationConfiguration(IConfiguration cache, IFieldMappings fieldMappings, SyncJobParameters syncJobParameters)
		{
			_cache = cache;
			_fieldMappings = fieldMappings;
			SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;
		}

		public string GetJobName() => _cache.GetFieldValue<RelativityObjectValue>(JobHistoryGuid).Name;

		public string GetNotificationEmails() => _cache.GetFieldValue(x => x.EmailNotificationRecipients);

		public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();

		public string GetFolderPathSourceFieldName() => _cache.GetFieldValue(x => x.FolderPathSourceFieldName);
	}
}