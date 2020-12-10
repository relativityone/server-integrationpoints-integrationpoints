using System.Collections.Generic;
using System.Linq;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Sync.Configuration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Storage;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
	internal class DocumentSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, IDocumentSyncConfigurationBuilder
	{
		private readonly IFieldsMappingBuilder _fieldsMappingBuilder;

		public DocumentSyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr,
				IFieldsMappingBuilder fieldsMappingBuilder, ISerializer serializer, DocumentSyncOptions options) 
			: base(syncContext, servicesMgr, serializer)
		{
			_fieldsMappingBuilder = fieldsMappingBuilder;

			SyncConfiguration.RdoArtifactTypeId = (int)ArtifactType.Document;
			SyncConfiguration.DataSourceType = DataSourceType.SavedSearch.ToString();
			SyncConfiguration.DataDestinationType = DestinationLocationType.Folder.ToString();
			SyncConfiguration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None.ToString();

			SyncConfiguration.DataSourceArtifactId = options.SavedSearchId;
			SyncConfiguration.DataDestinationArtifactId = options.DestinationFolderId;
			SyncConfiguration.NativesBehavior = options.CopyNativesMode.GetDescription();

			SetSyncConfigurationFieldsMapping(options.FieldsMapping);
		}

		public IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options)
		{
			DestinationFolderStructureCleanup();

			SyncConfiguration.DestinationFolderStructureBehavior = options.DestinationFolderStructure.ToString();
			
			if (options.DestinationFolderStructure == DestinationFolderStructureBehavior.ReadFromField)
			{
				using (var fieldManager = ServicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
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

		public new IDocumentSyncConfigurationBuilder OverwriteMode(OverwriteOptions options)
		{
			base.OverwriteMode(options);

			return this;
		}

		public new IDocumentSyncConfigurationBuilder EmailNotifications(EmailNotificationsOptions options)
		{
			base.EmailNotifications(options);

			return this;
		}

		public new IDocumentSyncConfigurationBuilder CreateSavedSearch(CreateSavedSearchOptions options)
		{
			base.CreateSavedSearch(options);

			return this;
		}

		public new IDocumentSyncConfigurationBuilder IsRetry(RetryOptions options)
		{
			base.IsRetry(options);

			return this;
		}

		#region Private methods
		private void SetSyncConfigurationFieldsMapping(List<FieldMap> fieldsMapping)
		{
			if (fieldsMapping != null && fieldsMapping.Any())
			{
				if (!FieldsMappingHasSingleIdentifierMap(fieldsMapping))
				{
					throw new InvalidSyncConfigurationException("Fields Mapping contains more than one Identifier map");
				}

				SyncConfiguration.FieldsMapping = Serializer.Serialize(fieldsMapping);
			}
			else
			{
				var defaultFieldsMapping = _fieldsMappingBuilder.WithIdentifier().FieldsMapping;
				SyncConfiguration.FieldsMapping = Serializer.Serialize(defaultFieldsMapping);
			}
		}

		private bool FieldsMappingHasSingleIdentifierMap(List<FieldMap> fieldsMapping)
		{
			return fieldsMapping.Count(x => x.SourceField.IsIdentifier
			                                || x.DestinationField.IsIdentifier
			                                || x.FieldMapType == FieldMapType.Identifier) == 1;
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
