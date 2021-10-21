using System;
using System.Threading.Tasks;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs.Framework;
using Relativity.Sync.SyncConfiguration.FieldsMapping;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

namespace Relativity.Sync.SyncConfiguration
{
    internal class NonDocumentSyncConfigurationBuilder : SyncConfigurationRootBuilderBase, INonDocumentSyncConfigurationBuilder
    {
        private readonly IFieldsMappingBuilder _fieldsMappingBuilder;
        private Action<IFieldsMappingBuilder> _fielsdMappingAction;

        public NonDocumentSyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr,
            IFieldsMappingBuilder fieldsMappingBuilder, ISerializer serializer, NonDocumentSyncOptions options,
            RdoOptions rdoOptions, RdoManager rdoManager) : base(syncContext, servicesMgr, rdoOptions, rdoManager, serializer)
        {
            _fieldsMappingBuilder = fieldsMappingBuilder;

            SyncConfiguration.RdoArtifactTypeId = options.RdoArtifactTypeId;
            SyncConfiguration.DataSourceType = DataSourceType.View;

            SyncConfiguration.DataSourceArtifactId = options.SourceViewArtifactId;
        }

        protected override async Task ValidateAsync()
        {
            SetFieldsMapping();
            await ValidateViewExistsAsync().ConfigureAwait(false);
        }

        private void SetFieldsMapping()
        {
            if (_fielsdMappingAction != null)
            {
                _fielsdMappingAction(_fieldsMappingBuilder);
                
                SyncConfiguration.FieldsMapping = Serializer.Serialize(
                    _fieldsMappingBuilder.FieldsMapping);
            }
        }

        private Task ValidateViewExistsAsync()
        {
            throw new System.NotImplementedException();
        }

        public INonDocumentSyncConfigurationBuilder WithFieldsMapping(Action<IFieldsMappingBuilder> fieldsMappingAction)
        {
            _fielsdMappingAction = fieldsMappingAction;
            return this;
        }
    }
}