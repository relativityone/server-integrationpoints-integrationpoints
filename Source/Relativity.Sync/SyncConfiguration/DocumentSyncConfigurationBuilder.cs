﻿using System;
using System.Threading.Tasks;
using Relativity.Services.Interfaces.Field;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
    internal class DocumentSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, IDocumentSyncConfigurationBuilder
    {
        private readonly IFieldsMappingBuilder _fieldsMappingBuilder;

        private Action<IFieldsMappingBuilder> _fieldsMappingAction;

        private DestinationFolderStructureOptions _destinationFolderStructureOptions;

        internal DocumentSyncConfigurationBuilder(
                ISyncContext syncContext,
                ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
                IFieldsMappingBuilder fieldsMappingBuilder,
                ISerializer serializer,
                DocumentSyncOptions options,
                RdoOptions rdoOptions,
                IRdoManager rdoManager)
            : base(syncContext, serviceFactoryForAdmin, rdoOptions, rdoManager, serializer)
        {
            _fieldsMappingBuilder = fieldsMappingBuilder;

            SyncConfiguration.RdoArtifactTypeId = (int)ArtifactType.Document;
            SyncConfiguration.DataSourceType = DataSourceType.SavedSearch;
            SyncConfiguration.DataDestinationType = DestinationLocationType.Folder;
            SyncConfiguration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None;

            SyncConfiguration.DataSourceArtifactId = options.SavedSearchId;
            SyncConfiguration.DataDestinationArtifactId = options.DestinationFolderId;
            SyncConfiguration.NativesBehavior = options.CopyNativesMode;

            SyncConfiguration.EnableTagging = options.EnableTagging;
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

        public new IDocumentSyncConfigurationBuilder CorrelationId(string correlationId)
        {
            base.CorrelationId(correlationId);

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

        public new IDocumentSyncConfigurationBuilder DisableItemLevelErrorLogging()
        {
            base.DisableItemLevelErrorLogging();
            return this;
        }

        protected override Task ValidateAsync()
        {
            SetFieldsMapping();
            ValidateSavedSearchCreation();

            return SetDestinationFolderStructureAsync();
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

        private void ValidateSavedSearchCreation()
        {
            if (SyncConfiguration.CreateSavedSearchInDestination && !SyncConfiguration.EnableTagging)
            {
                throw new InvalidSyncConfigurationException("Saved Search creation in destination workspace can't be configured when Tagging is disabled");
            }
        }

        private async Task SetDestinationFolderStructureAsync()
        {
            if (_destinationFolderStructureOptions == null)
            {
                return;
            }

            DestinationFolderStructureCleanup();

            SyncConfiguration.DestinationFolderStructureBehavior =
                _destinationFolderStructureOptions.DestinationFolderStructure;

            if (_destinationFolderStructureOptions.DestinationFolderStructure == DestinationFolderStructureBehavior.ReadFromField)
            {
                using (var fieldManager = await ServiceFactoryForAdmin.CreateProxyAsync<IFieldManager>())
                {
                    var folderPathField = await fieldManager.ReadAsync(
                            SyncContext.SourceWorkspaceId,
                            _destinationFolderStructureOptions.FolderPathSourceFieldId)
                        .ConfigureAwait(false);

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
            SyncConfiguration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None;
            SyncConfiguration.FolderPathSourceFieldName = null;
            SyncConfiguration.MoveExistingDocuments = false;
        }
        #endregion
    }
}
