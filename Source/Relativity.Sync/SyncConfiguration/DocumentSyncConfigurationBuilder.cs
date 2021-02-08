﻿using System;
using System.Threading.Tasks;
using Relativity.API;
using Relativity.Services.Interfaces.Field;
using Relativity.Sync.Configuration;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
	internal class DocumentSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, IDocumentSyncConfigurationBuilder
	{
		private readonly IFieldsMappingBuilder _fieldsMappingBuilder;

		private Action<IFieldsMappingBuilder> _fieldsMappingAction;
		
		private DestinationFolderStructureOptions _destinationFolderStructureOptions;

		public DocumentSyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr,
			IFieldsMappingBuilder fieldsMappingBuilder, ISerializer serializer, DocumentSyncOptions options,
			RdoOptions rdoOptions) 
			: base(syncContext, servicesMgr, rdoOptions, serializer)
		{
			_fieldsMappingBuilder = fieldsMappingBuilder;

			SyncConfiguration.RdoArtifactTypeId = (int)ArtifactType.Document;
			SyncConfiguration.DataSourceType = DataSourceType.SavedSearch.ToString();
			SyncConfiguration.DataDestinationType = DestinationLocationType.Folder.ToString();
			SyncConfiguration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None.ToString();

			SyncConfiguration.DataSourceArtifactId = options.SavedSearchId;
			SyncConfiguration.DataDestinationArtifactId = options.DestinationFolderId;
			SyncConfiguration.NativesBehavior = options.CopyNativesMode.GetDescription();
		}

		public IDocumentSyncConfigurationBuilder DestinationFolderStructure(DestinationFolderStructureOptions options)
		{
			_destinationFolderStructureOptions = options;

			return this;
		}

		public IDocumentSyncConfigurationBuilder WithFieldsMapping(Action<IFieldsMappingBuilder> fieldsMapping)
		{
			_fieldsMappingAction = fieldsMapping;

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

		protected override async Task ValidateAsync()
		{
			SetFieldsMapping();

			await SetDestinationFolderStructureAsync().ConfigureAwait(false);
		}

		#region Private methods

		private void SetFieldsMapping()
		{
			if (_fieldsMappingAction != null)
			{
				_fieldsMappingAction(_fieldsMappingBuilder);
			}
			else
			{
				_fieldsMappingBuilder.WithIdentifier();
			}

			SyncConfiguration.FieldsMapping = Serializer.Serialize(
				_fieldsMappingBuilder.FieldsMapping);
		}

		private async Task SetDestinationFolderStructureAsync()
		{
			if (_destinationFolderStructureOptions == null)
			{
				return;
			}

			DestinationFolderStructureCleanup();

			SyncConfiguration.DestinationFolderStructureBehavior = 
				_destinationFolderStructureOptions.DestinationFolderStructure.ToString();

			if (_destinationFolderStructureOptions.DestinationFolderStructure == DestinationFolderStructureBehavior.ReadFromField)
			{
				using (var fieldManager = ServicesMgr.CreateProxy<IFieldManager>(ExecutionIdentity.System))
				{
					var folderPathField = await fieldManager.ReadAsync(SyncContext.SourceWorkspaceId,
						_destinationFolderStructureOptions.FolderPathSourceFieldId).ConfigureAwait(false);

					if (folderPathField == null)
					{
						throw new InvalidSyncConfigurationException(
							$"Folder Path Field {_destinationFolderStructureOptions.FolderPathSourceFieldId} not found");
					}

					SyncConfiguration.FolderPathSourceFieldName = folderPathField.Name;
				}
			}

			if (_destinationFolderStructureOptions.DestinationFolderStructure != DestinationFolderStructureBehavior.None)
			{
				SyncConfiguration.MoveExistingDocuments = _destinationFolderStructureOptions.MoveExistingDocuments;
			}
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
