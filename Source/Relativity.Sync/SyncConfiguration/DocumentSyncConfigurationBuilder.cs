using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Sync.Configuration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Storage;

namespace Relativity.Sync.SyncConfiguration
{
	public class DocumentSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, IDocumentSyncConfigurationBuilder
	{
		public DocumentSyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr,
				IFieldsMappingBuilder fieldsMappingBuilder, DocumentSyncOptions options) 
			: base(syncContext, servicesMgr)
		{
			SyncConfiguration.RdoArtifactTypeId = (int)ArtifactType.Document;
			SyncConfiguration.DataSourceType = DataSourceType.SavedSearch.ToString(); //ToString ???
			SyncConfiguration.DestinationWorkspaceArtifactId = SyncContext.DestinationWorkspaceId;
			SyncConfiguration.DataDestinationType = DestinationLocationType.Folder.ToString(); //ToString ???
			SyncConfiguration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None.ToString(); //ToString ??? 

			SyncConfiguration.DataSourceArtifactId = options.SavedSearchId;
			SyncConfiguration.DataDestinationArtifactId = options.DestinationFolderId;
			SyncConfiguration.NativesBehavior = options.CopyNativesMode.GetDescription();

			var fieldsMapping = options.FieldsMapping != null && options.FieldsMapping.Any()
				? options.FieldsMapping
				: fieldsMappingBuilder.WithIdentifier().FieldsMapping;
			SyncConfiguration.FieldsMapping = Serializer.Serialize(fieldsMapping);
		}

		public IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options)
		{
			DestinationFolderStructureCleanup();

			SyncConfiguration.DestinationFolderStructureBehavior = options.DestinationFolderStructure.ToString();
			
			if (options.DestinationFolderStructure == DestinationFolderStructureBehavior.ReadFromField)
			{
				using (IFieldManager fieldManager = ServicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
				{
					var folderPathField = fieldManager.ReadAsync(SyncContext.SourceWorkspaceId, options.FolderPathSourceFieldId)
						.GetAwaiter().GetResult();

					SyncConfiguration.FolderPathSourceFieldName = folderPathField.Name;
				}
			}

			if (options.DestinationFolderStructure != DestinationFolderStructureBehavior.None)
			{
				SyncConfiguration.MoveExistingDocuments = options.MoveExistingDocuments;
			}

			return this;
		}

		public IDocumentSyncConfigurationBuilder OverwriteMode(OverwriteOptions options)
		{
			base.OverwriteMode(options);

			return this;
		}

		public IDocumentSyncConfigurationBuilder EmailNotifications(EmailNotificationsOptions options)
		{
			base.EmailNotifications(options);

			return this;
		}

		public IDocumentSyncConfigurationBuilder CreateSavedSearch(CreateSavedSearchOptions options)
		{
			base.CreateSavedSearch(options);

			return this;
		}

		public IDocumentSyncConfigurationBuilder IsRetry(RetryOptions options)
		{
			base.IsRetry(options);

			return this;
		}

		#region Private methods

		private void SetFieldsMapping(List<FieldMap> fieldsMapping)
		{

		}

		private void DestinationFolderStructureCleanup()
		{
			SyncConfiguration.DestinationFolderStructureBehavior = null;
			SyncConfiguration.FolderPathSourceFieldName = null;
			SyncConfiguration.MoveExistingDocuments = false;
		}

		#endregion
	}
}
