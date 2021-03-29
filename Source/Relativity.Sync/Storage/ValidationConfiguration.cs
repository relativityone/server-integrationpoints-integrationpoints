using System;
using System.Collections.Generic;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs.Framework;

namespace Relativity.Sync.Storage
{
    internal sealed class ValidationConfiguration : IValidationConfiguration
    {
        private readonly IConfiguration _cache;
        private readonly IFieldMappings _fieldMappings;
        private readonly Lazy<string> _jobNameLazy;

        public int SourceWorkspaceArtifactId { get; }

        public ValidationConfiguration(IConfiguration cache, IFieldMappings fieldMappings,
	        SyncJobParameters syncJobParameters, ISyncServiceManager servicesManager)
        {
	        _cache = cache;
	        _fieldMappings = fieldMappings;
	        SourceWorkspaceArtifactId = syncJobParameters.WorkspaceId;

	        _jobNameLazy = new Lazy<string>(() =>
	        {
		        using (var objectManager = servicesManager.CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser))
		        {
			        return objectManager.GetObjectNameAsync(syncJobParameters.WorkspaceId,
					        _cache.GetFieldValue(x => x.JobHistoryId),
					        _cache.GetFieldValue(x => x.JobHistoryType))
				        .GetAwaiter().GetResult();
		        }
	        });
        }
        
        public int DestinationWorkspaceArtifactId => _cache.GetFieldValue(x => x.DestinationWorkspaceArtifactId);

        public int SavedSearchArtifactId => _cache.GetFieldValue(x => x.DataSourceArtifactId);

        public int DestinationFolderArtifactId => _cache.GetFieldValue(x => x.DataDestinationArtifactId);

        public Guid JobHistoryObjectTypeGuid => _cache.GetFieldValue(x => x.JobHistoryType);

        public ImportOverwriteMode ImportOverwriteMode => (ImportOverwriteMode) Enum.Parse(typeof(ImportOverwriteMode),
            _cache.GetFieldValue(x => x.ImportOverwriteMode));

        public FieldOverlayBehavior FieldOverlayBehavior => _cache.GetFieldValue(x => x.FieldOverlayBehavior)
            .GetEnumFromDescription<FieldOverlayBehavior>();

        public DestinationFolderStructureBehavior DestinationFolderStructureBehavior =>
            (DestinationFolderStructureBehavior) Enum.Parse(typeof(DestinationFolderStructureBehavior),
                _cache.GetFieldValue(x => x.DestinationFolderStructureBehavior));

        public ImportNativeFileCopyMode ImportNativeFileCopyMode => _cache.GetFieldValue(x => x.NativesBehavior)
            .GetEnumFromDescription<ImportNativeFileCopyMode>();

        public ImportImageFileCopyMode ImportImageFileCopyMode => _cache.GetFieldValue(x => x.ImageFileCopyMode)
            .GetEnumFromDescription<ImportImageFileCopyMode>();

        public int? JobHistoryToRetryId => _cache.GetFieldValue(x => x.JobHistoryToRetryId);

        public string GetJobName() => _jobNameLazy.Value;

        public string GetNotificationEmails() => _cache.GetFieldValue(x => x.EmailNotificationRecipients);

        public IList<FieldMap> GetFieldMappings() => _fieldMappings.GetFieldMappings();

        public string GetFolderPathSourceFieldName() => _cache.GetFieldValue(x => x.FolderPathSourceFieldName);
        
        public bool Resuming => _cache.GetFieldValue(x => x.Resuming);
        
        public Guid? SnapshotId => _cache.GetFieldValue(x => x.SnapshotId);
    }
}