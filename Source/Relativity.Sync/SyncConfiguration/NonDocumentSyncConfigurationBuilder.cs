using System;
using System.Threading.Tasks;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
    internal class NonDocumentSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, INonDocumentSyncConfigurationBuilder
    {
	    private readonly ISyncContext _syncContext;
	    private readonly ISourceServiceFactoryForAdmin _serviceFactoryForAdmin;
	    private readonly IFieldsMappingBuilder _fieldsMappingBuilder;
        private Action<IFieldsMappingBuilder> _fieldsMappingAction;

        public NonDocumentSyncConfigurationBuilder(ISyncContext syncContext, ISourceServiceFactoryForAdmin serviceFactoryForAdmin,
            IFieldsMappingBuilder fieldsMappingBuilder, ISerializer serializer, NonDocumentSyncOptions options,
            RdoOptions rdoOptions, RdoManager rdoManager) : base(syncContext, serviceFactoryForAdmin, rdoOptions, rdoManager, serializer)
        {
	        _syncContext = syncContext;
	        _serviceFactoryForAdmin = serviceFactoryForAdmin;
	        _fieldsMappingBuilder = fieldsMappingBuilder;

            SyncConfiguration.RdoArtifactTypeId = options.RdoArtifactTypeId;
            SyncConfiguration.DestinationRdoArtifactTypeId = options.DestinationRdoArtifactTypeId;

            SyncConfiguration.DataSourceType = DataSourceType.View;
            SyncConfiguration.DataSourceArtifactId = options.SourceViewArtifactId;
        }

		public INonDocumentSyncConfigurationBuilder WithFieldsMapping(Action<IFieldsMappingBuilder> fieldsMappingAction)
        {
            _fieldsMappingAction = fieldsMappingAction;
            return this;
		}

		protected override async Task ValidateAsync()
        {
            SetFieldsMapping();
            await ValidateViewExistsAsync().ConfigureAwait(false);
        }

        private void SetFieldsMapping()
        {
            if (_fieldsMappingAction != null)
            {
                _fieldsMappingAction(_fieldsMappingBuilder);
                
                SyncConfiguration.FieldsMapping = Serializer.Serialize(
                    _fieldsMappingBuilder.FieldsMapping);
            }
        }

        private async Task ValidateViewExistsAsync()
        {
	        using (IObjectManager objectManager = await _serviceFactoryForAdmin.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
	        {
		        QueryRequest request = new QueryRequest()
		        {
			        ObjectType = new ObjectTypeRef()
			        {
				        ArtifactTypeID = (int)ArtifactType.View
			        },
			        Condition = $"'ArtifactID' == {SyncConfiguration.DataSourceArtifactId}"
		        };

		        QueryResult queryResult = await objectManager.QueryAsync(_syncContext.SourceWorkspaceId, request, 0, 1).ConfigureAwait(false);

		        if (queryResult.TotalCount == 0)
		        {
			        throw new InvalidSyncConfigurationException($"View Artifact ID: {SyncConfiguration.DataSourceArtifactId} does not exist in workspace {_syncContext.SourceWorkspaceId}");
		        }
	        }
        }

        public new INonDocumentSyncConfigurationBuilder CorrelationId(string correlationId)
        {
	        base.CorrelationId(correlationId);
	        return this;
        }

        public new INonDocumentSyncConfigurationBuilder OverwriteMode(OverwriteOptions options)
        {
	        base.OverwriteMode(options);
	        return this;
        }

        public new INonDocumentSyncConfigurationBuilder EmailNotifications(EmailNotificationsOptions options)
        {
	        base.EmailNotifications(options);
	        return this;
        }

        public new INonDocumentSyncConfigurationBuilder CreateSavedSearch(CreateSavedSearchOptions options)
        {
	        base.CreateSavedSearch(options);
	        return this;
        }

        public new INonDocumentSyncConfigurationBuilder IsRetry(RetryOptions options)
        {
	        base.IsRetry(options);
	        return this;
        }

        public new INonDocumentSyncConfigurationBuilder DisableItemLevelErrorLogging()
        {
	        base.DisableItemLevelErrorLogging();
	        return this;
        }
    }
}